using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Перечисление типов ингредиентов
public enum IngredientType
{
    Sous,
    Ananas,    
    Anchous,   
    Bazilik,
    Bekon,
    Brokli,
    Grib,
    Kabachok,
    KolbasaDoktor,
    KolbasaKapcha,
    KolbasaSred,
    Krevetka,
    Luk,
    LukKrasniy,
    Maslina,
    Olivka,
    PerecZH,
    PerecZ,
    PerecK,
    PerecOstr,
    Pomidor,
    Sir
}

public class Topping : MonoBehaviour
{
    public IngredientType type; // Выбирается в Инспекторе для каждого префаба
}
