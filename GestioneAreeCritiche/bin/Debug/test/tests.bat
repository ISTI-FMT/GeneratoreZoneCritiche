@echo off
echo > testres.txt
echo '----------------------'  >> testres.txt
echo 'missioni.txt'  >> testres.txt
echo '----------------------'  >> testres.txt
echo 'ok' | ..\GestioneAreeCritiche.exe missioni.txt
diff missioni.umc missioni.umc_bak >> testres.txt

echo '----------------------'  >> testres.txt
echo 'missioni_4.txt'  >> testres.txt
echo '----------------------'  >> testres.txt
echo 'ok' |..\GestioneAreeCritiche.exe missioni_4.txt
diff missioni_4.umc missioni_4.umc_bak >> testres.txt

echo '----------------------'  >> testres.txt
echo 'missioni_3.txt'  >> testres.txt
echo '----------------------'  >> testres.txt
echo 'ok' |..\GestioneAreeCritiche.exe missioni_3.txt
diff missioni_3.umc missioni_3.umc_bak >> testres.txt

echo '----------------------'  >> testres.txt
echo 'missioni_5.txt'  >> testres.txt
echo '----------------------'  >> testres.txt
echo 'ok' |..\GestioneAreeCritiche.exe missioni_5.txt 
diff missioni_5.umc missioni_5.umc_bak >> testres.txt

echo '----------------------'  >> testres.txt
echo '8treni.txt'  >> testres.txt
echo '----------------------'  >> testres.txt
echo 'ok' |..\GestioneAreeCritiche.exe 8treni.txt
diff 8treni.umc 8treni.umc_bak >> testres.txt

echo '----------------------'  >> testres.txt
echo 'doppiaentrata.txt'  >> testres.txt
echo '----------------------'  >> testres.txt
echo 'ok' |..\GestioneAreeCritiche.exe doppiaentrata.txt
diff doppiaentrata.umc doppiaentrata.umc_bak >> testres.txt

echo '----------------------'  >> testres.txt
echo 'twoway.txt'  >> testres.txt
echo '----------------------'  >> testres.txt
echo 'ok' |..\GestioneAreeCritiche.exe twoway.txt
diff twoway.umc twoway.umc_bak >> testres.txt