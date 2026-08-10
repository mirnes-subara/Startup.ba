# Dokumentacija sistema preporuke — Startup.ba

## Pregled

Startup.ba koristi **content-based** preporuke po kategoriji startup-a. Sistem ne koristi kolaborativno filtriranje niti offline treniranje ML modela: lista se računa **on-demand** pri svakom pozivu API-ja, na osnovu postojećih interakcija korisnika (like, favorite, donacija).

Implementacija: `StartupService.GetRecommendedStartupsAsync` u projektu `Startupba.Services`.

## API ulazna tačka

| Stavka | Vrijednost |
|--------|------------|
| Endpoint | `GET /Startup/recommended?count={n}` |
| Autorizacija | JWT Bearer (obavezno) |
| Korisnik | `userId` iz claim-a `ClaimTypes.NameIdentifier` |
| Default `count` | 5 (ako je ≤ 0) |

Kontroler: `StartupController.GetRecommendedStartups`.

## Signali interesa (težine)

Za svaki startup s kojim je korisnik već interagirao, sistem akumulira težinu na **kategoriju** tog startup-a:

| Interakcija | Težina |
|-------------|--------|
| Like (`StartupLikes`) | +3 |
| Favorite (`Favorites`) | +4 |
| Completed donation (`Donations.Status == "Completed"`) | +5 |

Jedan startup može doprinijeti više signala (npr. like + favorite). Težine po kategoriji se sabiraju u profil interesa: `Dictionary<categoryId, weight>`.

## Kandidati

U obzir dolaze samo startup-i koji zadovoljavaju:

- `IsActive == true`
- status **Approved**
- `FounderId !=` trenutni korisnik (ne preporučuje vlastite projekte)
- korisnik ih još nije like-ao, favorizirao niti donirao (completed)

## Bodovanje i sortiranje

Ako postoji barem jedna kategorija s težinom > 0:

```
Score = categoryWeight
      + (broj like-ova) * 0.1
      + (broj completed donacija) * 0.2
```

Zadržavaju se samo kandidati sa `Score > 0` (tj. kategorija mora biti u profilu interesa). Sortiranje: `Score` silazno, zatim `CreatedAt` silazno. Uzima se prvih `count` rezultata.

### Dopuna liste (fill)

Ako ima manje pogodaka od `count`, preostala mjesta se popunjavaju popularnim odobrenim startupima (među kandidatima koji još nisu u listi), sortirano po:

`likes + completedDonations`, zatim `CreatedAt`.

## Cold start

Ako korisnik nema nijednu relevantnu interakciju (prazan profil kategorija), vraća se top `count` popularnih odobrenih kandidata (ista popularnost metrika kao fill).

## Objašnjive preporuke (explainability)

Svaki element odgovora može imati polje `RecommendationReason`:

| Situacija | Poruka |
|-----------|--------|
| Pogodak po kategoriji | `Based on your interest in {CategoryName}` |
| Fill / cold start | `Popular approved startup` |

Razlog se prikazuje na mobilnom UI-ju u `StartupCard` (Home i Explore sekcije preporuka).

## Prikaz u aplikaciji

- **Home** — featured / recommended lista
- **Explore** — horizontalna sekcija preporuka (kada nema aktivnog filtera/pretrage)

Informativni dijalog u UI-ju sažima isto ponašanje za krajnjeg korisnika.

## Napomene

- Nema periodičnog job-a ni cache-iranog modela; svaki request ponovo čita interakcije i kandidata iz baze.
- Algoritam je namjerno jednostavan i transparentan radi seminarskih zahtjeva (content-based + objašnjivost), ne radi personalizacije na nivou pojedinačnih startup-a izvan kategorije i popularnosti.
