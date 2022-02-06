# Inverse Curve Sidebar Bot

## Pre-requisites:
.NET 6 see Step 2 [here](https://docs.microsoft.com/en-us/dotnet/iot/deployment).

## How to use
### Build the project first using

    dotnet build --configuration=Release

### Run using:

    dotnet run --configuration=Release --no-build --updateInterval 60 --botToken [bot token here] --pool [curve pool address] --i [from coin index] --j [to coin index] --web3 [web3 url] --nickname [bots nickname]

### or with PM2:

    pm2 start "dotnet run --configuration=Release --no-build --updateInterval 60 --botToken [bot token here] --pool [curve pool address] --i [from coin index] --j [to coin index] --web3 [web3 url] --nickname \"DOLA peg (ETH)\"" --name 3pooldola

## How to update:
### Standalone:

    git pull && dotnet build

### or with PM2:

    git pull && pm2 stop all && dotnet build && pm2 restart all
