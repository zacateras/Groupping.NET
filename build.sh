rm -r dist

dotnet build engine/Groupping.NET.csproj -o ../dist -c Release

dotnet dist/Groupping.NET.dll -f data/Pokemon.csv -o dist/Pokemon.csv.out -t dist/Pokemon.csv.ind.out -c "HP,Attack,Sp. Atk"
dotnet dist/Groupping.NET.dll -f data/Random.csv -o dist/Random.csv.out -t dist/Random.csv.ind.out -m Manhattan -k 10 -n 10 -l 150