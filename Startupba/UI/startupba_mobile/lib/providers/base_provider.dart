import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:http/http.dart' as http;
import 'package:http/http.dart';
import 'package:startupba_mobile/model/search_result.dart';
import 'package:startupba_mobile/providers/auth_provider.dart';

abstract class BaseProvider<T> with ChangeNotifier {
  static String? baseUrl;
  @protected
  String endpoint = "";

  BaseProvider(String endpoint) {
    this.endpoint = endpoint;
    baseUrl = const String.fromEnvironment(
      "baseUrl",
      defaultValue: "",
    );
    if (baseUrl == null || baseUrl!.isEmpty) {
      baseUrl = dotenv.env['baseUrl'] ?? "http://10.0.2.2:5130/";
    }
  }

  Future<Response> _authorized(Future<Response> Function() request) async {
    var response = await request();
    if (response.statusCode == 401) {
      final refreshed = await AuthProvider.tryRefresh();
      if (refreshed) {
        response = await request();
      }
    }
    return response;
  }

  Future<T?> getById(int id) async {
    var url = "$baseUrl$endpoint/$id";
    var uri = Uri.parse(url);

    var response = await _authorized(() => http.get(uri, headers: createHeaders()));
    if (isValidResponse(response)) {
      if (response.body.isEmpty) return null;
      var data = jsonDecode(response.body);
      return fromJson(data);
    } else {
      throw Exception("Unknown error");
    }
  }

  Future<SearchResult<T>> get({dynamic filter}) async {
    var url = "$baseUrl$endpoint";

    if (filter != null) {
      var queryString = getQueryString(filter);
      url = "$url?$queryString";
    }

    var uri = Uri.parse(url);
    var response = await _authorized(() => http.get(uri, headers: createHeaders()));

    if (isValidResponse(response)) {
      var data = jsonDecode(response.body);
      var result = SearchResult<T>();
      result.totalCount = data['totalCount'];
      result.items = List<T>.from(data["items"].map((e) => fromJson(e)));
      return result;
    } else {
      throw Exception("Unknown error");
    }
  }

  Future<T> insert(dynamic request) async {
    var url = "$baseUrl$endpoint";
    var uri = Uri.parse(url);
    var jsonRequest = jsonEncode(request);
    var response = await _authorized(
      () => http.post(uri, headers: createHeaders(), body: jsonRequest),
    );

    if (isValidResponse(response)) {
      var data = jsonDecode(response.body);
      return fromJson(data);
    } else {
      throw Exception("Unknown error");
    }
  }

  Future<T> update(int id, [dynamic request]) async {
    var url = "$baseUrl$endpoint/$id";
    var uri = Uri.parse(url);
    var jsonRequest = jsonEncode(request);
    var response = await _authorized(
      () => http.put(uri, headers: createHeaders(), body: jsonRequest),
    );

    if (isValidResponse(response)) {
      var data = jsonDecode(response.body);
      return fromJson(data);
    } else {
      throw Exception("Unknown error");
    }
  }

  Future<bool> delete(int id) async {
    var url = "$baseUrl$endpoint/$id";
    var uri = Uri.parse(url);
    var response = await _authorized(() => http.delete(uri, headers: createHeaders()));

    if (response.statusCode == 204) return true;
    if (isValidResponse(response)) {
      if (response.body.isEmpty) return true;
      var data = jsonDecode(response.body);
      return data == true;
    } else {
      throw Exception("Unknown error");
    }
  }

  T fromJson(data) {
    throw Exception("Method not implemented");
  }

  bool isValidResponse(Response response) {
    if (response.statusCode < 299) {
      return true;
    } else if (response.statusCode == 401) {
      throw Exception("Please check your credentials and try again.");
    } else {
      String message = "Something went wrong (${response.statusCode})";
      try {
        if (response.body.isNotEmpty) {
          final body = jsonDecode(response.body);
          if (body is Map) {
            if (body['errors'] != null && body['errors'] is Map) {
              final Map<String, dynamic> errorsMap = body['errors'];
              final List<String> errorMessages = [];
              errorsMap.forEach((key, value) {
                if (value is List && value.isNotEmpty) {
                  errorMessages.add(value.map((e) => e.toString()).join(", "));
                } else if (value is String) {
                  errorMessages.add(value);
                }
              });
              if (errorMessages.isNotEmpty) {
                message = errorMessages.join("\n");
              }
            } else if (body['userText'] != null) {
              message = body['userText'].toString();
            } else if (body['message'] != null) {
              message = body['message'].toString();
            } else if (body['title'] != null) {
              message = body['title'].toString();
            } else if (body['detail'] != null) {
              message = body['detail'].toString();
            }
          } else if (body is String && body.trim().isNotEmpty) {
            message = body;
          }
        }
      } catch (_) {
        if (response.body.trim().isNotEmpty) {
          message = response.body;
        }
      }
      throw Exception(message);
    }
  }

  Map<String, String> createHeaders() {
    final bearer = AuthProvider.token ?? "";
    return {
      "Content-Type": "application/json",
      "Authorization": "Bearer $bearer",
    };
  }

  String getQueryString(
    Map params, {
    String prefix = '&',
    bool inRecursion = false,
  }) {
    String query = '';
    params.forEach((key, value) {
      if (inRecursion) {
        if (key is int) {
          key = '[$key]';
        } else if (value is List || value is Map) {
          key = '.$key';
        } else {
          key = '.$key';
        }
      }
      if (value is String || value is int || value is double || value is bool) {
        var encoded = value;
        if (value is String) {
          encoded = Uri.encodeComponent(value);
        }
        query += '$prefix$key=$encoded';
      } else if (value is DateTime) {
        query += '$prefix$key=${value.toIso8601String()}';
      } else if (value is List) {
        for (var item in value) {
          var encoded = item;
          if (item is String) {
            encoded = Uri.encodeComponent(item);
          }
          query += '$prefix$key=$encoded';
        }
      } else if (value is Map) {
        value.forEach((k, v) {
          query += getQueryString(
            {k: v},
            prefix: '$prefix$key',
            inRecursion: true,
          );
        });
      }
    });
    return query;
  }
}
