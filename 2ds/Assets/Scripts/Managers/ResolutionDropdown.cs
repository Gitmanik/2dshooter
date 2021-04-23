using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Gitmanik.BaseCode
{
    class ResolutionDropdown : MonoBehaviour
    {
        public struct Resolution
        {
            public int Width, Height;
        }

        private TMP_Dropdown ResolutionList;

        public UnityEvent<Resolution> onChange;

        private List<TMP_Dropdown.OptionData> resolutions;
        public void Start()
        {
            ResolutionList = GetComponent<TMP_Dropdown>();
            ResolutionList.ClearOptions();
            resolutions = new List<UnityEngine.Resolution>(Screen.resolutions).ConvertAll((UnityEngine.Resolution x) => $"{x.width}x{x.height}").Distinct().ToList().ConvertAll(x => new TMP_Dropdown.OptionData(x));
            ResolutionList.AddOptions(resolutions);
            ResolutionList.SetValueWithoutNotify(resolutions.IndexOf(resolutions.Find(x => x.text == $"{Screen.width}x{Screen.height}")));
            ResolutionList.onValueChanged.RemoveAllListeners();
            ResolutionList.onValueChanged.AddListener(OnResolutionSet);

        }

        private void OnResolutionSet(int arg0)
        {
            var res = resolutions[arg0].text.Split('x');
            onChange?.Invoke(new Resolution { Width = Convert.ToInt32(res[0]), Height = Convert.ToInt32(res[1]) });
        }
    }
}