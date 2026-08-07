pipeline {
    agent any

    environment {
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
        DOTNET_NOLOGO = '1'
        DOCKER_IMAGE = 'vab-app'
        GIT_REPO = 'https://github.com/khadraouihiba02/Application-de-vente.git'
        APP_PROJECT = 'Application de vente/Application de vente.csproj'
        TEST_PROJECT = 'Application de vente.Tests/Application de vente.Tests.csproj'
    }

    stages {

        // ─── Étape 1 : Récupération du code source ────────────────────────────
        stage('📥 Checkout') {
            steps {
                echo '========================================'
                echo '  Récupération du code source GitHub   '
                echo '========================================'
                git branch: 'master', url: "${GIT_REPO}"
                echo "✅ Code source récupéré avec succès."
            }
        }

        // ─── Étape 2 : Restauration des dépendances NuGet ────────────────────
        stage('📦 Restore') {
            steps {
                echo '========================================'
                echo '  Restauration des packages NuGet       '
                echo '========================================'
                bat "dotnet restore \"${APP_PROJECT}\""
                bat "dotnet restore \"${TEST_PROJECT}\""
                echo "✅ Packages NuGet restaurés avec succès."
            }
        }

        // ─── Étape 3 : Compilation du projet ──────────────────────────────────
        stage('🔨 Build') {
            steps {
                echo '========================================'
                echo '  Compilation de la solution .NET 8     '
                echo '========================================'
                bat "dotnet build \"${APP_PROJECT}\" --configuration Release --no-restore"
                echo "✅ Projet compilé avec succès."
            }
        }

        // ─── Étape 4 : Exécution des tests unitaires ──────────────────────────
        stage('🧪 Unit Tests') {
            steps {
                echo '========================================'
                echo '  Exécution des tests unitaires xUnit   '
                echo '========================================'
                bat "dotnet test \"${TEST_PROJECT}\" --no-restore --verbosity normal --logger \"trx;LogFileName=test_results.trx\""
                echo "✅ Tests unitaires réussis."
            }
            post {
                always {
                    // Publier les résultats des tests dans Jenkins
                    junit allowEmptyResults: true, testResults: '**/test_results.trx'
                }
                failure {
                    echo "❌ Des tests ont échoué ! Le déploiement Docker est annulé."
                }
            }
        }

        // ─── Étape 5 : Build de l'image Docker ────────────────────────────────
        stage('🐳 Docker Build') {
            when {
                expression { currentBuild.result == null || currentBuild.result == 'SUCCESS' }
            }
            steps {
                echo '========================================'
                echo '  Construction de l image Docker        '
                echo '========================================'
                bat "docker build -t ${DOCKER_IMAGE}:latest -t ${DOCKER_IMAGE}:${BUILD_NUMBER} ."
                echo "✅ Image Docker '${DOCKER_IMAGE}:${BUILD_NUMBER}' créée avec succès."
            }
        }

        // ─── Étape 6 : Démarrage de l'application via Docker Compose ──────────
        stage('🚀 Deploy (Docker Compose)') {
            when {
                expression { currentBuild.result == null || currentBuild.result == 'SUCCESS' }
            }
            steps {
                echo '========================================'
                echo '  Démarrage de l application VAB        '
                echo '========================================'
                bat "docker-compose up -d --force-recreate"
                echo "✅ Application déployée. Accessible sur http://localhost:8081"
            }
        }
    }

    // ─── Notifications finales du pipeline ────────────────────────────────────
    post {
        success {
            echo ''
            echo '╔══════════════════════════════════════════╗'
            echo '║   ✅ PIPELINE RÉUSSI — VAB Déployée     ║'
            echo '╚══════════════════════════════════════════╝'
        }
        failure {
            echo ''
            echo '╔══════════════════════════════════════════╗'
            echo '║   ❌ PIPELINE ÉCHOUÉ — Vérifier logs    ║'
            echo '╚══════════════════════════════════════════╝'
        }
        always {
            cleanWs()
        }
    }
}
