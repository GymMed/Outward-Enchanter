using OutwardEnchanter.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OutwardEnchanter.Managers
{
    public class GUIManager
    {
        private static GUIManager _instance;

        private Dictionary<int, Enchantment> _enchantmentsDictionary;
        private Canvas _mainCanvas;
        private GUIMainCanvasManager _mainCanvasManager;

        private GUIManager()
        {
        }

        public static GUIManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GUIManager();

                return _instance;
            }
        }

        public Dictionary<int, Enchantment> EnchantmentsDictionary { get => _enchantmentsDictionary; set => _enchantmentsDictionary = value; }
        public Canvas MainCanvas { get => _mainCanvas; set => _mainCanvas = value; }
        public GUIMainCanvasManager MainCanvasManager { get => _mainCanvasManager; set => _mainCanvasManager = value; }

        public void CreateCanvas()
        {
            OutwardEnchanter.LogMessage("Initalizing Canvas..");
            GameObject CanvasPrefab = AssetsHelper.GetFromAssetBundle<GameObject>("OutwardEnchanter", "outwardenchanterbundle", "OutwardEnchanterCanvas");

            if (CanvasPrefab == null)
            {
                OutwardEnchanter.LogMessage("Failed to load outwardenchanterbundle Asset Bundle");
                return;
            }

            MainCanvas = GameObject.Instantiate(CanvasPrefab).GetComponent<Canvas>();
            MainCanvasManager = MainCanvas.gameObject.AddComponent<GUIMainCanvasManager>();
            GameObject.DontDestroyOnLoad(MainCanvas);
        }
    }
}
