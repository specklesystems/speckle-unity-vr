using System;
using Speckle.ConnectorUnity.Components;
using UnityEngine;
using VRSample.Speckle_Helpers;
using VRSample.UI.Controllers;

namespace VRSample.Interactions
{
    /// <summary>
    /// Behaviour for performing receive/send interactions with Speckle using <see cref="SelectionController"/>
    /// </summary>
    [RequireComponent(typeof(SelectionController), typeof(SpeckleReceiver), typeof(VRSender))]
    public class SpeckleInteractionMenu : MonoBehaviour
    {
        private SelectionController uiController;
        private SpeckleReceiver receiver;
        private VRSender sender;

        public int streamLimit = 15;
        public int branchLimit = 10;
        public int commitLimit = 10;

        void Awake()
        {
            uiController = GetComponent<SelectionController>();
            receiver = GetComponent<SpeckleReceiver>();
            sender = GetComponent<VRSender>();
        }

        void OnEnable()
        {
            FocusStreamSelection();
            receiver.Stream.StreamsLimit = streamLimit;
            receiver.Branch.BranchesLimit = branchLimit;
            receiver.Branch.CommitsLimit = commitLimit;
        }

        private void FocusAccountSelection()
        {
            void RefreshAction()
            {
                receiver.Account.RefreshOptions();
                FocusAccountSelection();
            }
            SetupNavigation(RefreshAction, Application.Quit, "Exit");
            uiController.GenerateOptions(receiver.Account, FocusStreamSelection);
        }
    
        private void FocusStreamSelection()
        {
            void RefreshAction()
            {
                receiver.Stream.RefreshOptions();
                FocusStreamSelection();
            }
            SetupNavigation(RefreshAction, FocusAccountSelection, "Accounts");
            uiController.GenerateOptions(receiver.Stream, FocusBranchSelection);
        }
    
        private void FocusBranchSelection()
        {
            void RefreshAction()
            {
                receiver.Branch.RefreshOptions();
                FocusBranchSelection();
            }
            SetupNavigation(RefreshAction, FocusStreamSelection, "Streams");
            uiController.GenerateOptions(receiver.Branch, FocusCommitSelection);
        }
    
        private void FocusCommitSelection()
        {
            void RefreshAction()
            {
                receiver.Commit.RefreshOptions();
                FocusCommitSelection();
            }
            SetupNavigation(RefreshAction, FocusBranchSelection, "Branches");
            uiController.ClearOptionViews();
            uiController.CreateSendView(() =>
            {
                StartCoroutine(sender.ConvertAndSend(receiver.Account.Client, receiver.Stream.Selected, receiver.Branch.Selected));
            });
            uiController.CreateOptionViews(receiver.Commit, () =>
            {
                StartCoroutine(receiver.ReceiveAndConvertRoutine(receiver, "Test"));
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
