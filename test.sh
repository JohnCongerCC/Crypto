rm -R ./lib/CryptoTools/bin; 
rm -R ./lib/CryptoTools/obj/; 
rm -R bin; 
rm -R obj;
dotnet restore; 
dotnet test -v n
read -p "Press [Enter] key..."