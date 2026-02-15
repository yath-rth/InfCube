using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
public class NameGenerator : MonoBehaviour
{
    public static NameGenerator instance;
    public int nounCharLength = 92907;
    public int adjectiveCharLength=3075;
    public int lastAdjLength = 4;
    public int lastNounLength= 8;
    public TextAsset nounsTextAsset;
    public TextAsset adjectivesTextAsset;

    private void Awake()
    {
        if(instance != null) Destroy(this);
        instance = this;
    }

    [ContextMenu("Generate Name")]
    public string GenerateName() {
        int adjCharStart = Random.Range(0, adjectiveCharLength-lastAdjLength);
        int nounCharStart = Random.Range(0, nounCharLength-lastNounLength);
        
        int adjectiveStart = adjectivesTextAsset.text.IndexOf(',',adjCharStart);
        int nounStart = nounsTextAsset.text.IndexOf(',', nounCharStart);
        int adjectiveEnd, nounEnd;
        if (adjectiveStart == adjectiveCharLength-lastAdjLength) {
            adjectiveEnd = adjectiveCharLength - 1;
        }
        if(nounStart == nounCharLength-lastNounLength) {
            nounEnd = nounCharLength - 1;
        }
        adjectiveEnd = adjectivesTextAsset.text.IndexOf(',',adjectiveStart + 1);
        nounEnd = nounsTextAsset.text.IndexOf(',', nounStart + 1);
        string adjective = adjectivesTextAsset.text.Substring(adjectiveStart + 1, adjectiveEnd - adjectiveStart - 1);
        string noun = nounsTextAsset.text.Substring(nounStart + 1, nounEnd - nounStart - 1);
        Debug.Log($"{TitleCaseString(adjective)}{TitleCaseString(noun)}");
        return $"{TitleCaseString(adjective)}{TitleCaseString(noun)}";
    }

    public static String TitleCaseString(String s)
    {
        if (s == null) return s;

        String[] words = s.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length == 0) continue;

            Char firstChar = Char.ToUpper(words[i][0]);
            String rest = "";
            if (words[i].Length > 1)
            {
                rest = words[i].Substring(1).ToLower();
            }
            words[i] = firstChar + rest;
        }
        return String.Join(" ", words);
    }
}
