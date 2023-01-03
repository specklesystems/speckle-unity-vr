using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VRSample.UI.Components
{
    /// <summary>
    /// Generic UI component for option selection.
    /// </summary>
    /// <seealso cref="VRSample.UI.Controllers.SelectionController"/>
    public sealed class OptionViewComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public RawImage Image { get; private set; }
        public TMP_Text TitleText { get; private set; }
        public TMP_Text DescriptionText { get; private set; }
        public TMP_Text SelectButtonText { get; private set; }
        public Button SelectButton { get; private set; }

        private GameObject hoverGroup;


        public void Initialise(string title, string description, Action onClickHandler, string selectButtonText = "Select")
        {
            TitleText.text = title;
            DescriptionText.text = description;
            SelectButtonText.text = selectButtonText;
            SelectButton.onClick.AddListener(onClickHandler.Invoke);
        }
        
        
        // public void Initialise(Stream streamDetails, Texture previewImage, Action<string, Commit> onClickHandler)
        // {
        //     Image.texture = previewImage;
        //     TitleText.text = streamDetails.name;
        //     var UpdatedAt = FormatTime(DateTime.Parse(streamDetails.updatedAt));
        //     DescriptionText.text = $"Updated {UpdatedAt} ago";
        //
        //     List<TMP_Dropdown.OptionData> branchOptions = new(streamDetails.branches.items.Count);
        //     foreach (Branch b in streamDetails.branches.items)
        //     {
        //         branchOptions.Add(new TMP_Dropdown.OptionData(b.name));
        //     }
        //
        //     
        //     SelectButton.onClick.AddListener(() =>
        //     {
        //         Debug.Log("button pressed");
        //
        //         Branch selectedBranch = streamDetails.branches.items.FirstOrDefault(b => b.name == selectedBranchName);
        //         Commit selectedCommit = selectedBranch?.commits.items.FirstOrDefault();
        //         if (selectedCommit == null) return;
        //         
        //         onClickHandler(streamDetails.id, selectedCommit);
        //     });
        // }

        // protected static string FormatTime(DateTime updatedAt)
        // {
        //     TimeSpan dt = DateTime.Now - updatedAt;
        //     
        //     if (dt.Days == 1) return $"{dt.Days} day";
        //     if (dt.Days >= 1) return $"{dt.Days} days";
        //     if (dt.Hours == 1) return $"{dt.Hours} hour";
        //     if (dt.Hours >= 1) return $"{dt.Hours} hours";
        //     if(dt.Minutes == 1) return $"{dt.Minutes} minute";
        //     if(dt.Minutes >= 1) return $"{dt.Minutes} minutes";
        //     return $"{dt.Seconds} seconds";
        // }
        
        void Awake()
        {
            //TODO: update
            SelectButton = GetComponentsInChildren<Button>(true).First(c => c.name == "Button");
            SelectButtonText = GetComponentsInChildren<TMP_Text>(true).First(c => c.name == "Button Text");
            TitleText = GetComponentsInChildren<TMP_Text>(true).First(c => c.name == "View Name Text");
            DescriptionText = GetComponentsInChildren<TMP_Text>(true).First(c => c.name == "View Description Text");
            Image = GetComponentsInChildren<RawImage>(true).First(c => c.name == "View Preview Image");
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
