allprojects {
    repositories {
        google()
        mavenCentral()
    }
}

val newBuildDir: Directory =
    rootProject.layout.buildDirectory
        .dir("../../build")
        .get()
rootProject.layout.buildDirectory.value(newBuildDir)

subprojects {
    val newSubprojectBuildDir: Directory = newBuildDir.dir(project.name)
    project.layout.buildDirectory.value(newSubprojectBuildDir)
}
subprojects {
    project.evaluationDependsOn(":app")
}

subprojects {
    val configureAndroid = {
        val android = project.extensions.findByName("android")
        if (android != null) {
            try {
                val setCompileSdk = android.javaClass.getMethod("compileSdkVersion", Int::class.javaPrimitiveType)
                setCompileSdk.invoke(android, 36)
            } catch (e: Exception) {
            }
        }
    }
    if (project.state.executed) {
        configureAndroid()
    } else {
        project.afterEvaluate { configureAndroid() }
    }
}

// Stripe's issuing-push-provisioning pulls private play-services-tapandpay,
// which breaks lintVitalAnalyzeRelease. Skip those lint tasks for release APKs.
subprojects {
    tasks.configureEach {
        if (name.contains("lintVital")) {
            enabled = false
        }
    }
}

tasks.register<Delete>("clean") {
    delete(rootProject.layout.buildDirectory)
}
