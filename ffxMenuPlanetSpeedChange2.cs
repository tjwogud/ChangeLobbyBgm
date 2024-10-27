using DG.Tweening;
using UnityEngine;

namespace ChangeLobbyBgm
{
    public class ffxMenuPlanetSpeedChange2 : ffxBase
    {
        private bool changed = false;

        public override void Awake()
        {
            base.Awake();
            floor.topGlow.enabled = false;
            floor.floorIcon = FloorIcon.Rabbit;
            floor.UpdateIconSprite();
            ctrl.speed = Main.Settings.defaultBpm / 100d;
            if (Main.Settings.customMusic)
            {
                Main.LoadMusic(Main.Settings.defaultMusicPath, true);
                scrConductor.instance.song.volume = 1;
                scrConductor.instance.song.pitch = 1;
                Main.LoadMusic(Main.Settings.fastMusicPath, false);
                scrConductor.instance.song2.volume = 0;
                scrConductor.instance.song2.pitch = 1;
            }
        }

        public void Start()
        {
            floor.topGlow.gameObject.SetActive(false);
            floor.bottomGlow.gameObject.SetActive(false);
        }
        
        public override void doEffect()
        {
            if (changed)
            {
                ctrl.speed = Main.Settings.defaultBpm / 100d;
                floor.floorIcon = FloorIcon.Rabbit;
                cond.song.DOKill();
                cond.song2.DOKill();
                if (Main.Settings.customMusic)
                {
                    cond.song.DOFade(1, 0.2f);
                    cond.song2.DOFade(0, 0.2f);
                }
                else
                    cond.song2.DOFade(0, 0.2f);
                changed = false;
            }
            else
            {
                ctrl.speed = Main.Settings.fastBpm / 100d;
                floor.floorIcon = FloorIcon.Snail;
                cond.song.DOKill();
                cond.song2.DOKill();
                if (Main.Settings.fastMusic)
                    if (Main.Settings.customMusic)
                    {
                        cond.song.DOFade(0, 0.2f);
                        cond.song2.DOFade(1, 0.2f);
                    }
                    else
                        cond.song2.DOFade(0.7f, 0.2f);
                changed = true;
            }
            floor.UpdateIconSprite();
        }
    }
}
