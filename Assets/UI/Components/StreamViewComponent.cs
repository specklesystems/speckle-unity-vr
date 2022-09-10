using System;
using System.Collections.Generic;
using System.Linq;
using Speckle.Core.Api;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VRSample.UI.Components
{
    public class StreamViewComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public RawImage PreviewImage { get; protected set; }
        public TMP_Text TitleText { get; protected set; }

        public TMP_Dropdown BranchSelection { get; protected set; }
        public TMP_Text UpdatedAtText { get; protected set; }
        public Button Button { get; protected set; }

        private GameObject hoverGroup;
        public void Initialise(Stream streamDetails, Texture previewImage, Action<string, Commit> onClickHandler)
        {
            PreviewImage.texture = previewImage;
            TitleText.text = streamDetails.name;
            var UpdatedAt = FormatTime(DateTime.Parse(streamDetails.updatedAt));
            UpdatedAtText.text = $"Updated {UpdatedAt} ago";

            List<TMP_Dropdown.OptionData> branchOptions = new(streamDetails.branches.items.Count);
            foreach (Branch b in streamDetails.branches.items)
            {
                branchOptions.Add(new TMP_Dropdown.OptionData(b.name));
            }

            BranchSelection.options = branchOptions;
            if(branchOptions.Count > 0) BranchSelection.value = 0;
            
            Button.onClick.AddListener(() =>
            {
                Debug.Log("button pressed");
                string selectedBranchName = BranchSelection.options[BranchSelection.value].text;
                Branch selectedBranch = streamDetails.branches.items.FirstOrDefault(b => b.name == selectedBranchName);
                Commit selectedCommit = selectedBranch?.commits.items.FirstOrDefault();
                if (selectedCommit == null) return;
                
                onClickHandler(streamDetails.id, selectedCommit);
            });
        }

        protected static string FormatTime(DateTime updatedAt)
        {
            TimeSpan dt = DateTime.Now - updatedAt;
            
            if (dt.Days == 1) return $"{dt.Days} day";
            if (dt.Days >= 1) return $"{dt.Days} days";
            if (dt.Hours == 1) return $"{dt.Hours} hour";
            if (dt.Hours >= 1) return $"{dt.Hours} hours";
            if(dt.Minutes == 1) return $"{dt.Minutes} minute";
            if(dt.Minutes >= 1) return $"{dt.Minutes} minutes";
            return $"{dt.Seconds} seconds";
        }
        
        void Awake()
        {
            Button = GetComponentsInChildren<Button>(true).First(c => c.name == "Receive Button");;
            TitleText = GetComponentsInChildren<TMP_Text>(true).First(c => c.name == "Stream Name Text");
            UpdatedAtText = GetComponentsInChildren<TMP_Text>(true).First(c => c.name == "Stream Updated Text");
            PreviewImage = GetComponentsInChildren<RawImage>(true).First(c => c.name == "Stream Preview Image");
            BranchSelection =  GetComponentsInChildren<TMP_Dropdown>(true).First(c => c.name == "Branch Selection Dropdown");
            hoverGroup = GetComponentsInChildren<Transform>(true).First(c => c.name == "Hover").gameObject;
            hoverGroup.SetActive(false);
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            hoverGroup.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hoverGroup.SetActive(false);
        }
    }
    
}
