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
        stage('Checkout') {
            steps {
                echo 'Recuperation du code source depuis GitHub...'
                git branch: 'master', url: "${GIT_REPO}"
                echo 'Code source recupere avec succes.'
            }
        }

        // ─── Étape 2 + 3 + 4 : Restore, Build, Test dans un conteneur .NET 8 ─
        stage('Restore - Build - Test') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:8.0'
                    reuseNode true
                    args '-v /root/.nuget/packages:/root/.nuget/packages'
                }
            }
            stages {
                stage('Restore') {
                    steps {
                        echo '--- Restauration des packages NuGet ---'
                        sh "dotnet restore \"${APP_PROJECT}\""
                        sh "dotnet restore \"${TEST_PROJECT}\""
                        echo 'Packages NuGet restaures avec succes.'
                    }
                }

                stage('Build') {
                    steps {
                        echo '--- Compilation de la solution .NET 8 ---'
                        sh "dotnet build \"${APP_PROJECT}\" --configuration Release --no-restore"
                        echo 'Projet compile avec succes.'
                    }
                }

                stage('Unit Tests') {
                    steps {
                        echo '--- Execution des tests unitaires xUnit ---'
                        sh "dotnet test \"${TEST_PROJECT}\" --no-restore --verbosity normal --logger \"trx;LogFileName=test_results.trx\""
                        echo 'Tests unitaires reussis.'
                    }
                    post {
                        always {
                            junit allowEmptyResults: true, testResults: '**/test_results.trx'
                        }
                        failure {
                            echo 'Des tests ont echoue ! Le deploiement Docker est annule.'
                        }
                    }
                }
            }
        }

        // ─── Étape 5 : Build de l'image Docker ────────────────────────────────
        stage('Docker Build') {
            when {
                expression { currentBuild.result == null || currentBuild.result == 'SUCCESS' }
            }
            steps {
                echo '--- Construction de l image Docker ---'
                sh "docker build -t ${DOCKER_IMAGE}:latest -t ${DOCKER_IMAGE}:${BUILD_NUMBER} ."
                echo "Image Docker ${DOCKER_IMAGE}:${BUILD_NUMBER} creee."
            }
        }

        // ─── Étape 6 : Déploiement via Docker Compose ─────────────────────────
        stage('Deploy') {
            when {
                expression { currentBuild.result == null || currentBuild.result == 'SUCCESS' }
            }
            steps {
                echo '--- Demarrage de l application VAB ---'
                sh "docker-compose up -d --force-recreate"
                echo 'Application deployee. Accessible sur http://localhost:8081'
            }
        }
    }

    post {
        success {
            echo 'PIPELINE REUSSI --- VAB Deployee avec succes'
        }
        failure {
            echo 'PIPELINE ECHOUE --- Verifier les logs ci-dessus'
        }
        always {
            deleteDir()
        }
    }
}
