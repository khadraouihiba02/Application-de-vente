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

        // ─── Étape 2 à 5 : Code Quality (Sonar), Build & Test ────────────────
        stage('Code Quality & Build') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:8.0'
                    reuseNode true
                    args '-v /root/.nuget/packages:/root/.nuget/packages'
                }
            }
            environment {
                SONAR_TOKEN = 'sqp_af989df0b88c413e7fdcb82b9cd9a70a63a95e4e'
                SONAR_HOST_URL = 'http://host.docker.internal:9000'
            }
            stages {
                stage('Prepare SonarScanner') {
                    steps {
                        echo '--- Installation de Java (requis pour Sonar) et SonarScanner ---'
                        sh '''
                            apt-get update && apt-get install -y default-jre
                            dotnet tool install --global dotnet-sonarscanner || true
                        '''
                    }
                }

                stage('Restore & Sonar Begin') {
                    steps {
                        echo '--- Demarrage de l analyse SonarQube ---'
                        sh '''
                            export PATH="$PATH:/root/.dotnet/tools"
                            dotnet sonarscanner begin \\
                                /k:"VAB" \\
                                /d:sonar.host.url="${SONAR_HOST_URL}" \\
                                /d:sonar.token="${SONAR_TOKEN}" \\
                                /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" \\
                                /d:sonar.cs.vstest.reportsPaths="**/test_results.trx" \\
                                /d:sonar.exclusions="**/coveragereport/**,**/TestResults/**,**/*.html,**/*.htm,**/wwwroot/lib/**,**/Migrations/**" \\
                                /d:sonar.coverage.exclusions="**/Migrations/**,**/Program.cs,**/wwwroot/**"
                            dotnet restore "${APP_PROJECT}"
                            dotnet restore "${TEST_PROJECT}"
                        '''
                    }
                }

                stage('Build') {
                    steps {
                        echo '--- Compilation de la solution .NET 8 ---'
                        sh "dotnet build \"${APP_PROJECT}\" --configuration Release --no-restore"
                    }
                }

                stage('Unit Tests') {
                    steps {
                        echo '--- Execution des tests unitaires avec couverture de code ---'
                        sh '''
                            dotnet test "${TEST_PROJECT}" \\
                                --no-restore \\
                                --verbosity normal \\
                                --logger "trx;LogFileName=test_results.trx" \\
                                --collect:"XPlat Code Coverage" \\
                                -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
                        '''
                    }
                    post {
                        always {
                            junit allowEmptyResults: true, testResults: '**/test_results.trx'
                        }
                    }
                }

                stage('Sonar End') {
                    steps {
                        echo '--- Envoi des resultats a SonarQube ---'
                        sh '''
                            export PATH="$PATH:/root/.dotnet/tools"
                            dotnet sonarscanner end /d:sonar.token="${SONAR_TOKEN}"
                        '''
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
                sh '''
                    echo "Nettoyage des anciens conteneurs..."
                    docker rm -f vab_sqlserver vab_web_app || true

                    if ! command -v docker-compose &> /dev/null; then
                        curl -sSL "https://github.com/docker/compose/releases/download/v2.24.5/docker-compose-$(uname -s)-$(uname -m)" -o ./docker-compose
                        chmod +x ./docker-compose
                        ./docker-compose up -d --force-recreate
                    else
                        docker-compose up -d --force-recreate
                    fi
                '''
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
