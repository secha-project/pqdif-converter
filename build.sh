#!/bin/bash

set -e

if [ -z "$1" ]; then
    target_arch="linux-x64"
else
    target_arch="$1"
fi

echo "Building self-contained application for ${target_arch}"
echo "================================================="
dotnet publish \
    -c Release \
    -r ${target_arch} \
    -f net9.0 \
    --self-contained \
    -p:PublishSingleFile=true \
    pqdif-converter/pqdif-converter.csproj

mkdir -p ./bin/${target_arch}
cp ./pqdif-converter/bin/Release/net9.0/${target_arch}/publish/* ./bin/${target_arch}
rm -f ./bin/${target_arch}/*.pdb
