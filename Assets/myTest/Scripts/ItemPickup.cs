﻿using System;
using Photon.Pun;
using UnityEngine;

namespace myTest
{
    [Serializable]
    public abstract class ItemPickup : MonoBehaviourPun, IPickup
    {
        [SerializeField] private Material highlightMaterial = null;

        private MeshRenderer _meshRenderer = null;
        private Material _baseMaterial = null;

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        private void Start()
        {
            _baseMaterial = _meshRenderer.material;

        }

        public void Highlight(bool isHighlight)
        {
            _meshRenderer.material = isHighlight ? highlightMaterial : _baseMaterial;
        }

        public abstract string ItemName { get; }

        public abstract void GrantPickup(PickupBehavior pickupBehavior);
    }
}
