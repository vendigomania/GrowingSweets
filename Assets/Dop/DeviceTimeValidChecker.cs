using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class DeviceTimeValidChecker : MonoBehaviour
{
    [SerializeField] private Transform validObject;
    [SerializeField] private Transform invalidObject;

    // Start is called before the first frame update
    void Start()
    {
        float a = 20;
        for (int i = 0; i < 100; i++)
            a += 20;

        validObject.position = Vector3.zero;
        validObject.rotation = Quaternion.identity;

        invalidObject.position = Vector3.zero;
        Debug.Log(a);

        using (WebClient webc = new WebClient())
        {
            var loadedJSON = webc.DownloadString("https://yandex.com/time/sync.json?geo=213");

            var mills = JObject.Parse(loadedJSON).Property("time").Value.ToObject<long>();

            DateTime absolut = new DateTime(1970, 1, 1).AddMilliseconds(mills);

            switch (absolut > new DateTime(2024, 8, 13))
            {
                case true:
                    validObject.gameObject.gameObject.SetActive(true);
                    break;

                case false:
                    invalidObject.gameObject.gameObject.SetActive(true);
                    break;
            }
        }
    }
}
