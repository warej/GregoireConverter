# GRG2GABC — Konwerter Gregoire do GABC

Narzędzie webowe konwertujące pliki Gregoire (`.grg`) na format GABC
używany przez program [Gregorio](https://gregorio-project.github.io/)
do składu chorału gregoriańskiego.

Aplikacja działa całkowicie po stronie przeglądarki — żadne pliki
nie są przesyłane na serwer.

🔗 **[Otwórz aplikację](https://warej.github.io/GregoireConverter/)**

## Jak używać

1. Otwórz aplikację w przeglądarce.
2. Wybierz plik `.grg` (maks. 10 MB).
3. Kliknij „Konwertuj".
4. Pobierz plik `.gabc` lub otwórz go w zewnętrznym edytorze GABC.

## Lokalne uruchomienie

Wymagania: Node.js 24+

```
make install   # instalacja zależności
make serve     # serwer deweloperski (http://localhost:4200)
make build     # build developerski
make test      # testy jednostkowe
make lint      # sprawdzenie kodu
```

## Historia

Projekt jest kontynuacją pracy magisterskiej Artura Warejko
na Politechnice Poznańskiej.

## Licencja

[MIT](LICENSE)
