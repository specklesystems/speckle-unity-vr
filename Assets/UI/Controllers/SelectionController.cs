using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Speckle.ConnectorUnity.Utils;
using Speckle.ConnectorUnity.Wrappers.Selection;
using Speckle.Core.Credentials;
using UnityEngine;
using UnityEngine.UI;
using VRSample.UI.Components;
using Text = TMPro.TMP_Text;

namespace VRSample.UI.Controllers
{
    /// <summary>
    /// Reusable UI Controller for selecting account/stream/branch/commit options 
    /// </summary>
    /// <seealso cref="OptionSelection{TOption}"/>
    [DisallowMultipleComponent]
    public class SelectionController : MonoBehaviour
    {
        [field: SerializeField]
        public Transform ContentTarget { get; set; }
        
        [field: SerializeField]
        public OptionViewComponent OptionViewPrefab { get; set; }
        
        [field: SerializeField]
        public Texture SendIcon { get; set; }
        
        public Button BackButton { get; private set; }
        public Text BackText { get; private set; }
        public Button RefreshButton { get; private set; }
        
        public void GenerateOptions<T>(OptionSelection<T> optionSelection, Action onSelect) where T : class
        {
            ClearOptionViews();
            
            CreateOptionViews(optionSelection, onSelect);
        }
        
        public OptionViewComponent CreateSendView(Action onSelect)
        {
            string title = "Create new commit";
            string description = "Send current scene as a new commit";

            OptionViewComponent optionElement = Instantiate(OptionViewPrefab, ContentTarget);
            optionElement.Initialise(title, description, () =>
            {
                onSelect.Invoke();
            }, "Send");
            SetImage(optionElement, SendIcon);
            return optionElement;
        }
        
        public List<OptionViewComponent> CreateOptionViews<T>(OptionSelection<T> optionSelection, Action callback) where T : class
        {
            List<OptionViewComponent> views = new List<OptionViewComponent>(optionSelection.Options.Length);
            
            for (int i = 0; i < optionSelection.Options.Length; i++)
            {
                //TODO not the cleanest solution, but for now it's ok
                var view = optionSelection switch
                {
                    AccountSelection a => CreateAccountView(a, i, callback),
                    StreamSelection s => CreateStreamView(s, i, callback),
                    BranchSelection b => CreateBranchView(b, i, callback),
                    CommitSelection c => CreateCommitView(c, i, callback),
                    _ => throw new ArgumentException($"Argument of unexpected type: {optionSelection.GetType()}",
                        nameof(optionSelection))
                };
                views.Add(view);
            }

            return views;
        }
        
        
        public void ClearOptionViews()
        {
            foreach (var child in ContentTarget.GetComponentsInChildren<OptionViewComponent>())
            {
                Destroy(child.gameObject);
            }
        }
        
        private static void SetImage(OptionViewComponent optionElement, Texture image)
        {
            optionElement.Image.texture = image;
            
            var rect = optionElement.Image.rectTransform;
            var width = (40 * 4) * ((float)image.width / image.height);
            var height = rect.sizeDelta.y;
            rect.sizeDelta = new Vector2(width, height);
        }
        
        private OptionViewComponent CreateAccountView(AccountSelection selection, int index, Action onSelect)
        {
            Account a = selection.Options[index];
            var serverInfo = a.serverInfo;
            var userInfo = a.userInfo;
            string title = $"{serverInfo.name} | {serverInfo.url}";
            string description = $"{userInfo.name} | {userInfo.email}";
            
            Texture2D avatar = new Texture2D (1, 1);
    
            avatar.LoadImage(Convert.FromBase64String(userInfo.avatar.Split(',')[^1]));
            avatar.Apply();

            OptionViewComponent optionElement = Instantiate(OptionViewPrefab, ContentTarget);
            optionElement.Initialise(title, description, () =>
            {
                selection.SelectedIndex = index;
                onSelect.Invoke();
            });
            SetImage(optionElement, avatar);
            return optionElement;
        }

        private OptionViewComponent CreateStreamView(StreamSelection selection, int index, Action onSelect)
        {
            var s = selection.Options[index];
            string title = $"{s.name} | {s.id}";
            string description = $"Updated {FormatTime(s.updatedAt)} ago : {s.description}";
            string imageurl = $"{selection.Client.ServerUrl}/preview/{s.id}";
            
            OptionViewComponent optionElement = Instantiate(OptionViewPrefab, ContentTarget);
            optionElement.Initialise(title, description, () =>
            {
                selection.SelectedIndex = index;
                onSelect.Invoke();
            });
            StartCoroutine(Utils.GetImageRoutine(imageurl,
                selection.Client.ApiToken, 
                t => SetImage(optionElement, t))
            );
            return optionElement;
        }
        
        private OptionViewComponent CreateBranchView(BranchSelection selection, int index, Action onSelect)
        {
            var stream = selection.StreamSelection.Selected;
            var b = selection.Options[index];
            string title = b.name;
            string description = b.description;
            string imageurl = $"{selection.Client.ServerUrl}/preview/{stream.id}/branches/{b.name}";
            
            OptionViewComponent optionElement = Instantiate(OptionViewPrefab, ContentTarget);
            optionElement.Initialise(title, description, () =>
            {
                selection.SelectedIndex = index;
                onSelect.Invoke();
            });
            StartCoroutine(Utils.GetImageRoutine(imageurl,
                selection.Client.ApiToken, 
                t => SetImage(optionElement, t))
            );
            return optionElement;
        }
        
        private OptionViewComponent CreateCommitView(CommitSelection selection, int index, Action onSelect)
        {
            var stream = selection.BranchSelection.StreamSelection.Selected;
            var c = selection.Options[index];
            string title = $"{c.message} | {c.id}";
            string description = $"{c.sourceApplication} - {FormatTime(DateTime.Parse(c.createdAt))} ago";
            string imageurl = $"{selection.Client.ServerUrl}/preview/{stream.id}/commits/{c.id}";
            
            OptionViewComponent optionElement = Instantiate(OptionViewPrefab, ContentTarget);
            optionElement.Initialise(title, description, () =>
            {
                selection.SelectedIndex = index;
                onSelect.Invoke();
            }, "Receive");
            StartCoroutine(Utils.GetImageRoutine(imageurl,
                selection.Client.ApiToken, 
                t => SetImage(optionElement, t))
            );
            return optionElement;
        }
        
        
        void Awake()
        {
            var buttons = GetComponentsInChildren<Button>();
            RefreshButton = buttons.First(b => b.name == "Refresh Button");
            BackButton = buttons.First(b => b.name == "Back Button");
            BackText = BackButton.GetComponentInChildren<Text>();
        }
        void OnDisable()
        {
            ClearOptionViews();
        }

        public static string FormatTime(string time)
        {
            return FormatTime(DateTime.Parse(time));
        }
        
        public static string FormatTime(DateTime updatedAt)
        {
            TimeSpan dt = DateTime.Now - updatedAt;
            
            if (dt.Days >= 2) return $"{dt.Days} days";
            if (dt.Hours == 1) return $"{dt.Hours} hour";
            if (dt.Hours >= 1) return $"{dt.Hours} hours";
            if (dt.Minutes == 1) return $"{dt.Minutes} minute";
            if (dt.Minutes >= 1) return $"{dt.Minutes} minutes";
            return $"{dt.Seconds} seconds";
        }
        
    }
}


