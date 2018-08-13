﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BlockEnhancementMod;
using System.Reflection;
using System;

namespace BlockEnhancementMod.Blocks
{
    public class CannonScript : EnhancementBlock
    {

        public MSlider StrengthSlider;

        public MSlider IntervalSlider;

        public MSlider RandomDelaySlider;

        public MSlider KnockBackSpeedSlider;

        public MToggle BulletToggle;

        public MToggle InheritSizeToggle;

        public MSlider BulletMassSlider;

        public MSlider BulletDragSlider;

        public MToggle TrailToggle;

        public MSlider TrailLengthSlider;

        public MColourSlider TrailColorSlider;

        public CanonBlock CB;

        public AudioSource AS;

        public float Strength = 1f;

        public float Interval = 0.25f;

        public float RandomDelay = 0.2f;

        public float KnockBackSpeed = 1f;

        public float originalKnockBackSpeed = 0;

        public bool cBullet = false;

        public bool InheritSize = false;

        public float BulletMass = 2f;

        public float BulletDrag = 0.2f;

        public bool Trail = false;

        public float TrailLength = 1f;

        public Color TrailColor = Color.yellow;



        protected override void SafeAwake()
        {

            IntervalSlider = AddSlider(LanguageManager.fireInterval, "Interval", Interval, 0f, 0.5f, false);
            IntervalSlider.ValueChanged += (float value) => { Interval = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { Interval = IntervalSlider.Value; };

            RandomDelaySlider = AddSlider(LanguageManager.randomDelay, "RandomDelay", RandomDelay, 0f, 0.5f, false);
            RandomDelaySlider.ValueChanged += (float value) => { RandomDelay = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { RandomDelay = RandomDelaySlider.Value; };

            KnockBackSpeedSlider = AddSlider(LanguageManager.recoil, "KnockBackSpeed", KnockBackSpeed, 0.2f, 1f, false);
            KnockBackSpeedSlider.ValueChanged += (float value) => { KnockBackSpeed = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { KnockBackSpeed = KnockBackSpeedSlider.Value; };

            BulletToggle = AddToggle(LanguageManager.customBullet, "Bullet", cBullet);
            BulletToggle.Toggled += (bool value) => { BulletDragSlider.DisplayInMapper = BulletMassSlider.DisplayInMapper = InheritSizeToggle.DisplayInMapper = cBullet = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { cBullet = BulletToggle.IsActive; };

            InheritSizeToggle = AddToggle(LanguageManager.inheritSize, "InheritSize", InheritSize);
            InheritSizeToggle.Toggled += (bool value) => { InheritSize = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { InheritSize = InheritSizeToggle.IsActive; };

            BulletMassSlider = AddSlider(LanguageManager.bulletMass, "BulletMass", BulletMass, 0.1f, 2f, false);
            BulletMassSlider.ValueChanged += (float value) => { BulletMass = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { BulletMass = BulletMassSlider.Value; };

            BulletDragSlider = AddSlider(LanguageManager.bulletDrag, "BulletDrag", BulletDrag, 0.01f, 0.5f, false);
            BulletDragSlider.ValueChanged += (float value) => { BulletDrag = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { BulletDrag = BulletDragSlider.Value; };

            TrailToggle = AddToggle(LanguageManager.trail, "Trail", Trail);
            TrailToggle.Toggled += (bool value) => { Trail = TrailColorSlider.DisplayInMapper = TrailLengthSlider.DisplayInMapper = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { Trail = TrailToggle.IsActive; };

            TrailLengthSlider = AddSlider(LanguageManager.trailLength, "trail length", TrailLength, 0.2f, 2f, false);
            TrailLengthSlider.ValueChanged += (float value) => { TrailLength = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { TrailLength = TrailLengthSlider.Value; };

            TrailColorSlider = AddColorSlider(LanguageManager.trailColor, "trail color", TrailColor, false);
            TrailColorSlider.ValueChanged += (Color value) => { TrailColor = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { TrailColor = TrailColorSlider.Value; };

            // Initialise some components and default values
            AS = BB.GetComponent<AudioSource>();
            CB = BB.GetComponent<CanonBlock>();
            originalKnockBackSpeed = CB.knockbackSpeed;



#if DEBUG
            //ConsoleController.ShowMessage("加农炮添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {

            IntervalSlider.DisplayInMapper = value;
            RandomDelaySlider.DisplayInMapper = value;
            KnockBackSpeedSlider.DisplayInMapper = value;
            BulletToggle.DisplayInMapper = value && !StatMaster.isMP;
            InheritSizeToggle.DisplayInMapper = value && cBullet;
            BulletMassSlider.DisplayInMapper = value && cBullet;
            BulletDragSlider.DisplayInMapper = value && cBullet;

            TrailColorSlider.DisplayInMapper = Trail;
            TrailLengthSlider.DisplayInMapper = Trail;

        }



        /// <summary>
        /// 子弹刚体组件
        /// </summary>
        public Rigidbody BR;

        public TrailRenderer myTrailRenderer;

        public GameObject BulletObject;

        public float Drag;

        float timer;

        private float knockBackSpeed;

        private int BulletNumber = 1;

        private GameObject customBulletObject;

        protected override void OnSimulateStart()
        {

            Strength = CB.StrengthSlider.Value;

            BulletObject = CB.boltObject.gameObject;
            //BR = BulletObject.GetComponent<Rigidbody>();

            //BulletSpeed = (CB.boltSpeed * Strength) / 15f;
            knockBackSpeed = KnockBackSpeed * originalKnockBackSpeed;

            CB.enabled = !cBullet;
            timer = Interval;

            //独立自定子弹
            if (cBullet)
            {

                customBulletObject = (GameObject)Instantiate(BulletObject, CB.boltSpawnPos.position, CB.boltSpawnPos.rotation);
                customBulletObject.transform.localScale = !InheritSize ? new Vector3(0.5f, 0.5f, 0.5f) : Vector3.Scale(Vector3.one * Mathf.Min(transform.localScale.x, transform.localScale.z), new Vector3(0.5f, 0.5f, 0.5f));
                customBulletObject.SetActive(false);
                if (InheritSize)
                {
                    CB.particles[0].transform.localScale = customBulletObject.transform.localScale;
                }
                BR = customBulletObject.GetComponent<Rigidbody>();
                BR.mass = BulletMass < 0.1f ? 0.1f : BulletMass;
                BR.drag = BR.angularDrag = Drag;

            }
            else
            {
                CB.randomDelay = RandomDelay;
                CB.knockbackSpeed = knockBackSpeed;
            }

            GameObject bullet = cBullet ? customBulletObject : BulletObject;

            if (Trail)
            {

                if (bullet.GetComponent<TrailRenderer>() == null)
                {
                    myTrailRenderer = bullet.AddComponent<TrailRenderer>();
                }
                else
                {
                    myTrailRenderer = bullet.GetComponent<TrailRenderer>();
                    myTrailRenderer.enabled = Trail;
                }
                myTrailRenderer.autodestruct = false;
                myTrailRenderer.receiveShadows = false;
                myTrailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                myTrailRenderer.startWidth = 0.5f * bullet.transform.localScale.magnitude;
                myTrailRenderer.endWidth = 0.1f;
                myTrailRenderer.time = TrailLength;

                myTrailRenderer.material = new Material(Shader.Find("Particles/Additive"));
                myTrailRenderer.material.SetColor("_TintColor", TrailColor);
            }
            else
            {
                myTrailRenderer = bullet.GetComponent<TrailRenderer>();
                if (myTrailRenderer)
                {
                    myTrailRenderer.enabled = Trail;
                }
            }

            //全局自定子弹

            //CB.randomDelay = RandomDelay;
            //CB.knockbackSpeed = knockBackSpeed;

            //if (cBullet)
            //{

            //    BR.mass = BulletMass;
            //    BR.drag = BR.angularDrag = Drag;

            //    BulletObject.transform.localScale = !InheritSize ? new Vector3(0.5f, 0.5f, 0.5f) : Vector3.Scale(Vector3.one * Mathf.Min(transform.localScale.x, transform.localScale.z), new Vector3(0.5f, 0.5f, 0.5f));
            //}



        }

        protected override void OnSimulateUpdate()
        {
            if (CB.ShootKey.IsDown && cBullet)
            {
                CB.StopAllCoroutines();
            }
            if (CB.ShootKey.IsDown && Interval > 0)
            {
                if (timer > Interval)
                {
                    timer = 0;
                    if (cBullet)
                    {
                        StartCoroutine(Shoot());
                    }
                    else
                    {
                        CB.Shoot();
                    }
                }
                else
                {
                    timer += Time.deltaTime;
                }
            }
            else if (CB.ShootKey.IsReleased)
            {
                timer = Interval;
            }
        }

        private IEnumerator Shoot()
        {
            if (BulletNumber > 0)
            {
                float randomDelay = UnityEngine.Random.Range(0f, RandomDelay);

                yield return new WaitForSeconds(randomDelay);

                var bullet = (GameObject)Instantiate(customBulletObject, CB.boltSpawnPos.position, CB.boltSpawnPos.rotation);

                bullet.SetActive(true);
                try { bullet.GetComponent<Rigidbody>().velocity = CB.Rigidbody.velocity; }
                catch { }
                //bullet.GetComponent<Rigidbody>().AddForce(-transform.up * CB.boltSpeed * Strength / Mathf.Min(customBulletObject.transform.localScale.x, customBulletObject.transform.localScale.z));
                bullet.GetComponent<Rigidbody>().AddForce(-transform.up * CB.boltSpeed * Strength);

                gameObject.GetComponent<Rigidbody>().AddForce(knockBackSpeed * Strength * Mathf.Min(customBulletObject.transform.localScale.x, customBulletObject.transform.localScale.z) * transform.up);

                foreach (var particle in CB.particles)
                {
                    particle.Play();
                }
                AS.Play();
                CB.fuseParticles.Stop();

            }

            if (!StatMaster.GodTools.InfiniteAmmoMode)
            {
                BulletNumber--;
            }
            else
            {
                BulletNumber = 1;
            }

        }



    }


}


