using System;
using Speckle.ConnectorUnity.Components;
using Speckle.ConnectorUnity.Wrappers.Selection;
using UnityEngine;
using VRSample.Speckle_Helpers;
using VRSample.UI.Controllers;

namespace VRSample.Interactions
{
    /// <summary>
    /// Behaviour for performing receive/send interactions with Speckle using <see cref="SelectionController"/>
    /// </summary>
    [RequireComponent(typeof(SelectionController), typeof(VRReceiver), typeof(VRSender))]
    public class SpeckleInteractionMenu : MonoBehaviour
    {
        private SelectionController uiController;
        private VRSender sender;
        private VRReceiver receiver;

        public int streamLimit = 15;
        public int branchLimit = 10;
        public int commitLimit = 10;
        public GameObject environment;

        private AccountSelection Account => receiver.Receiver.Account;
        private StreamSelection Stream => receiver.Receiver.Stream;
        private BranchSelection Branch => receiver.Receiver.Branch;
        private CommitSelection Commit => receiver.Receiver.Commit;
        
        void Awake()
        {
            uiController = GetComponent<SelectionController>();
            receiver = GetComponent<VRReceiver>();
            sender = GetComponent<VRSender>();
        }

        void Start()
        {
            FocusStreamSelection();
            Stream.StreamsLimit = streamLimit;
            Branch.BranchesLimit = branchLimit;
            Branch.CommitsLimit = commitLimit;
        }

        private void FocusAccountSelection()
        {
            void RefreshAction()
            {
                Account.RefreshOptions();
                FocusAccountSelection();
            }
            SetupNavigation(RefreshAction, Application.Quit, "Exit");
            uiController.GenerateOptions(Account, FocusStreamSelection);
        }
    
        private void FocusStreamSelection()
        {
            void RefreshAction()
            {
                Stream.RefreshOptions();
                FocusStreamSelection();
            }
            SetupNavigation(RefreshAction, FocusAccountSelection, "Accounts");
            uiController.GenerateOptions(Stream, FocusBranchSelection);
        }
    
        private void FocusBranchSelection()
        {
            void RefreshAction()
            {
                Branch.RefreshOptions();
                FocusBranchSelection();
            }
            SetupNavigation(RefreshAction, FocusStreamSelection, "Streams");
            uiController.GenerateOptions(Branch, FocusCommitSelection);
        }
    
        private void FocusCommitSelection()
        {
            void RefreshAction()
            {
                Commit.RefreshOptions();
                FocusCommitSelection();
            }
            SetupNavigation(RefreshAction, FocusBranchSelection, "Branches");
            uiController.ClearOptionViews();
            uiController.CreateSendView(() =>
            {
                StartCoroutine(sender.ConvertAndSend(environment, Account.Client, Stream.Selected,Branch.Selected));
            });
            uiController.CreateOptionViews(Commit, () =>
            {
                StartCoroutine(receiver.ReceiveRoutine(environment.transform));
            });
        }

        private void SetupNavigation(Action refresh, Action back, string backText = "Back")
        {
            //ideally this class would not be directly interfacing TMP elements
            uiController.RefreshButton.onClick.RemoveAllListeners();
            uiController.RefreshButton.onClick.AddListener(refresh.Invoke);
        
            uiController.BackButton.onClick.RemoveAllListeners();
            uiController.BackButton.onClick.AddListener(back.Invoke);
            uiController.BackText.text = backText;
        }
    }
}
