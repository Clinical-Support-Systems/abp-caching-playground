#!/bin/bash

if [[ ! -d certs ]]
then
    mkdir certs
    cd certs/
    if [[ ! -f localhost.pfx ]]
    then
        dotnet dev-certs https -v -ep localhost.pfx -p 9ab04747-6430-42d9-a539-73c3ba57a9c3 -t
    fi
    cd ../
fi

docker-compose up -d
