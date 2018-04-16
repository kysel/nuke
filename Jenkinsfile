pipeline {
    agent {label 'linux' }
    options {
        buildDiscarder(logRotator(numToKeepStr:'10'))
        timeout(time: 15, unit: 'MINUTES')
    }
    stages {
        stage('Compile') {
            steps {
                sh '/bin/bash ./build.sh Restore'
                sh '/usr/bin/mono /home/jenkins/.nuget/packages/gitversion.commandline/3.6.5/tools/GitVersion.exe'
            }
        }
      
        
    }
}
