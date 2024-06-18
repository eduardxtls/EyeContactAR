// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using RiptideNetworking;
using RiptideNetworking.Utils;
using RiptideNetworking.Transports;
using System;
using System.CodeDom;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.SampleQRCodes
{
    public class QRCodesVisualizer : MonoBehaviour
    {
        public GameObject qrCodePrefab;

        // Custom
        public GameObject anchorPrefab;
        Vector3 anchorPosition;
        bool anchorPlaced = false;
        KeyValuePair<System.Guid, GameObject> QR = new KeyValuePair<Guid, GameObject>();
        // End custom

        private SortedDictionary<System.Guid, GameObject> qrCodesObjectsList;
        public SortedDictionary<System.Guid, GameObject> qrCodesObjectsList_copy; // Copy to avoid overwriting

        private bool clearExisting = false;

        struct ActionData
        {
            public enum Type
            {
                Added,
                Updated,
                Removed
            };
            public Type type;
            public Microsoft.MixedReality.QR.QRCode qrCode;

            public ActionData(Type type, Microsoft.MixedReality.QR.QRCode qRCode) : this()
            {
                this.type = type;
                qrCode = qRCode;
            }
        }

        private Queue<ActionData> pendingActions = new Queue<ActionData>();

        // Use this for initialization
        void Start()
        {
            Debug.Log("QRCodesVisualizer start");
            qrCodesObjectsList = new SortedDictionary<System.Guid, GameObject>();
            qrCodesObjectsList_copy = new SortedDictionary<System.Guid, GameObject>();

            QRCodesManager.Instance.QRCodesTrackingStateChanged += Instance_QRCodesTrackingStateChanged;
            QRCodesManager.Instance.QRCodeAdded += Instance_QRCodeAdded;
            QRCodesManager.Instance.QRCodeUpdated += Instance_QRCodeUpdated;
            QRCodesManager.Instance.QRCodeRemoved += Instance_QRCodeRemoved;
            if (qrCodePrefab == null)
            {
                throw new System.Exception("Prefab not assigned");
            }
        }

        public void startVisual()
        {
            QRCodesManager.Instance.QRCodesTrackingStateChanged += Instance_QRCodesTrackingStateChanged;
            QRCodesManager.Instance.QRCodeAdded += Instance_QRCodeAdded;
            QRCodesManager.Instance.QRCodeUpdated += Instance_QRCodeUpdated;
            QRCodesManager.Instance.QRCodeRemoved += Instance_QRCodeRemoved;
        }
        
        public void stopVisual()
        {
            QRCodesManager.Instance.QRCodesTrackingStateChanged -= Instance_QRCodesTrackingStateChanged;
            QRCodesManager.Instance.QRCodeAdded -= Instance_QRCodeAdded;
            QRCodesManager.Instance.QRCodeUpdated -= Instance_QRCodeUpdated;
            QRCodesManager.Instance.QRCodeRemoved -= Instance_QRCodeRemoved;
        }

        private void Instance_QRCodesTrackingStateChanged(object sender, bool status)
        {
            if (!status)
            {
                clearExisting = true;
            }
        }

        private void Instance_QRCodeAdded(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeAdded");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Added, e.Data));
            }
        }

        private void Instance_QRCodeUpdated(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeUpdated");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Updated, e.Data));
            }
        }

        private void Instance_QRCodeRemoved(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            Debug.Log("QRCodesVisualizer Instance_QRCodeRemoved");

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Removed, e.Data));
            }
        }

        private void HandleEvents()
        {
            lock (pendingActions)
            {
                while (pendingActions.Count > 0)
                {
                    var action = pendingActions.Dequeue();
                    if (action.type == ActionData.Type.Added)
                    {
                        GameObject qrCodeObject = Instantiate(qrCodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                        qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
                        qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
                        qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);
                        qrCodesObjectsList_copy.Add(action.qrCode.Id, qrCodeObject);
                    }
                    else if (action.type == ActionData.Type.Updated)
                    {
                        if (!qrCodesObjectsList.ContainsKey(action.qrCode.Id))
                        {
                            GameObject qrCodeObject = Instantiate(qrCodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                            qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
                            qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
                            qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);
                            qrCodesObjectsList_copy.Add(action.qrCode.Id, qrCodeObject);
                        }
                    }
                    else if (action.type == ActionData.Type.Removed)
                    {
                        if (qrCodesObjectsList.ContainsKey(action.qrCode.Id))
                        {
                            Destroy(qrCodesObjectsList[action.qrCode.Id]);
                            qrCodesObjectsList.Remove(action.qrCode.Id);
                            qrCodesObjectsList_copy.Remove(action.qrCode.Id);
                        }
                    }
                }
            }

            // START CUSTOM --------------------------------

            stopVisual();
            lock (qrCodesObjectsList_copy)
            {
                foreach (KeyValuePair<System.Guid, GameObject> keyValuePair in qrCodesObjectsList_copy)
                {
                    Debug.Log("QR Code " + keyValuePair.Value.GetComponent<QRCode>().qrCode.Data + " detected.");
                    if (keyValuePair.Value.GetComponent<QRCode>().qrCode.Data == "ECA" && !anchorPlaced)
                    {
                        
                        Debug.Log("Anchor QR Code found");
                        QR = keyValuePair;

                        //anchorPrefab = TransformCam.Singleton.anchor;
                        //TransformCam.Singleton.anchor = anchorPrefab; // Set TransformCam anchor to this anchor
                        // Instantiate the anchor ???

                        // Place the anchor at this QR code's position and rotation
                        
                       
                        anchorPrefab.transform.position = QR.Value.transform.position;
                        anchorPrefab.transform.rotation = QR.Value.transform.rotation;

                        // Prevents multiple placements of anchor
                        //anchorPlaced = true;
                        TransformCam.Singleton.transformCam();
                        Debug.Log("Transformed"+ TransformCam.Singleton.RelativePos);
                        break;
                    }
                }
            }

            startVisual(); // Start visualization again

            // END CUSTOM --------------------------------

            if (clearExisting)
            {
                clearExisting = false;
                foreach (var obj in qrCodesObjectsList)
                {
                    Destroy(obj.Value);
                }
                qrCodesObjectsList.Clear();

                foreach (var obj in qrCodesObjectsList_copy)
                {
                    Destroy(obj.Value);
                }
                qrCodesObjectsList_copy.Clear();

            }
        }

        // Update is called once per frame
        void Update()
        {
            HandleEvents();
        }
    }
}
