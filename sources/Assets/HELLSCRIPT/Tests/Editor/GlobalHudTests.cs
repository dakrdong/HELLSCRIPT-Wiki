using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript.Tests
{
    public sealed class GlobalHudTests
    {
        // Every HUD text row holds its proportional font, including at the smallest captured windows.
        [Test] public void EveryHudTextRowHoldsOneLineOfItsOwnFont()
        {
            var style=GlobalHudStyle.Load();
            var host=new GameObject("hud row",typeof(RectTransform));
            var label=host.AddComponent<Text>();
            // A dynamic OS font left behind keeps its atlas alive for the rest of the editor session and
            // has been seen to change what a later test reads back, so this one is destroyed with the row.
            var font=GameUI.CreateFont();bool created=font!=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.font=font;label.text="보호막 +25";
            label.horizontalOverflow=HorizontalWrapMode.Overflow;label.verticalOverflow=VerticalWrapMode.Overflow;
            try
            {
                foreach(var size in new[]{new Vector2Int(360,640),new Vector2Int(640,360),new Vector2Int(900,360),new Vector2Int(540,960),new Vector2Int(720,1280),new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(900,1600),new Vector2Int(3440,1440)})
                foreach(int percent in new[]{50,85,100,140,150})
                {
                    var p=new GlobalHudLayout(size.x,size.y,InterfaceScaleOptions.Factor(percent),style);
                    string where=" at "+size.x+"x"+size.y+" @"+percent+"%";
                    foreach(var row in new[]{("Level",p.level,style.levelFont,style.minimumLevelFont),("HP",p.hp,style.valueFont,style.minimumValueFont),
                        ("Resource",p.resource,style.valueFont,style.minimumValueFont),("Shield",p.shield,style.shieldFont,style.minimumFont),("XP",p.xpText,style.captionFont,style.minimumFont)})
                    {
                        label.fontSize=p.FontSize(row.Item3,(int)row.Item4);
                        Assert.That(label.fontSize/p.scale,Is.EqualTo(Mathf.Max(row.Item3,row.Item4)).Within(.5f/p.scale+.001f),row.Item1+" text no longer proportional"+where);
                        Assert.That(label.preferredHeight,Is.LessThanOrEqualTo(row.Item2.height*p.scale+1),row.Item1+" text clipped"+where);
                    }
                    Assert.That(p.shield.yMax,Is.LessThanOrEqualTo(p.status.y),"Shield band runs into the status strip"+where);
                    Assert.That(p.shield.y,Is.GreaterThanOrEqualTo(p.shieldLine.yMax),"Shield text sits on its own line"+where);
                }
            }
            finally{Object.DestroyImmediate(host);if(created&&font!=null)Object.DestroyImmediate(font);}
        }
        [Test] public void LandscapeLocksApprovedGeometry()
        {
            var p=new GlobalHudLayout(1600,900);
            Assert.That(p.scale,Is.EqualTo(1));Assert.That(p.passives.Length,Is.EqualTo(3));Assert.That(p.actives.Length,Is.EqualTo(4));
            Assert.That(p.actives.All(r=>Mathf.Abs(r.width-76.8f)<.001f),Is.True);
            Assert.That(p.potions.All(r=>r.width==45&&r.y>p.actives[0].yMax),Is.True);
            Assert.That(p.actives[0].x-p.passives[2].xMax,Is.EqualTo(24).Within(.001));
            Assert.That(p.status.width,Is.EqualTo(230));Assert.That(p.maximumStatusWidth,Is.EqualTo(408));
            Assert.That(p.xp,Is.EqualTo(new Rect(32,16,1536,3)));
        }
        [Test] public void HalvingTheWindowHalvesTextAndControlsAcrossRepeatedRefreshes()
        {
            var large=new GlobalHudLayout(1600,900);var small=new GlobalHudLayout(800,450);
            Assert.That(small.Pixels(small.actives[0]).width/large.Pixels(large.actives[0]).width,Is.EqualTo(.5f).Within(.001));
            foreach(float font in new[]{12f,16f,20f,22f})
            {
                int expected=large.FontSize(font,10)/2;
                for(int refresh=0;refresh<30;refresh++)Assert.That(new GlobalHudLayout(800,450).FontSize(font,10),Is.EqualTo(expected));
            }
        }
        [Test] public void PortraitKeepsSeparateRowsAndShortXp()
        {
            var p=new GlobalHudLayout(900,1600);
            Assert.That(p.passives.All(r=>r.y>p.actives[0].yMax),Is.True);
            Assert.That(p.potions.All(r=>r.y>p.passives[0].yMax),Is.True);
            Assert.That(p.actives[0].width,Is.EqualTo(64));Assert.That(p.potions[0].width,Is.EqualTo(60));Assert.That(p.xp.width,Is.EqualTo(260));
        }
        [TestCase(640,360,.5f)] [TestCase(640,360,1.5f)] [TestCase(360,640,1.5f)] [TestCase(1200,900,1.5f)] [TestCase(2000,900,1)]
        public void AllSevenSkillsAndThreePotionsStayInside(int w,int h,float scale)
        {
            var p=new GlobalHudLayout(w,h,scale);
            foreach(var r in p.passives.Concat(p.actives).Concat(p.potions))
            {Assert.That(r.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(r.yMin,Is.GreaterThanOrEqualTo(0));Assert.That(r.xMax,Is.LessThanOrEqualTo(p.width+.01f));Assert.That(r.yMax,Is.LessThanOrEqualTo(p.height+.01f));}
            for(int a=0;a<p.actives.Length;a++)foreach(var passive in p.passives)Assert.That(p.actives[a].Overlaps(passive),Is.False);
            foreach(var potion in p.potions)foreach(var skill in p.actives.Concat(p.passives))
                Assert.That(potion.yMin,Is.GreaterThan(skill.yMax),"Potions must stay above every skill row.");
        }
        [Test] public void FadesOnlyHiddenEdgesAndApproachesBoundaryContinuously()
        {
            Assert.That(GlobalHudLayout.FadeWidths(0,100),Is.EqualTo(new Vector2(0,22)));
            Assert.That(GlobalHudLayout.FadeWidths(50,100),Is.EqualTo(new Vector2(22,22)));
            Assert.That(GlobalHudLayout.FadeWidths(100,100),Is.EqualTo(new Vector2(22,0)));
            Assert.That(GlobalHudLayout.FadeWidths(99,100).y,Is.EqualTo(1));Assert.That(GlobalHudLayout.FadeWidths(0,0),Is.EqualTo(Vector2.zero));
            Assert.That(GlobalHudLayout.EdgeAlpha(0,22),Is.Zero);Assert.That(GlobalHudLayout.EdgeAlpha(11,22),Is.EqualTo(.5f));Assert.That(GlobalHudLayout.EdgeAlpha(22,22),Is.EqualTo(1));
        }
        [TestCase(440,956)] [TestCase(956,440)] [TestCase(1600,900)] [TestCase(1600,1000)] [TestCase(2100,900)]
        [TestCase(360,640)]
        public void BottomAnchoredActionsAndPotionTrayDoNotOverlapVitals(int width,int height)
        {
            var style=GlobalHudStyle.Load();
            foreach(float factor in new[]{.5f,1,1.5f})
            {
                var p=new GlobalHudLayout(width,height,factor,style);
                float baseline=p.landscape?Mathf.Max(style.skillBottom,style.xpBottom+style.xpThickness+style.captionHeight+8):style.skillBottom;
                Assert.That(p.actives[0].yMin,Is.EqualTo(baseline),"The bottom row must not float upward in portrait.");
                if(p.landscape)Assert.That(p.actives[0].yMin-style.captionHeight,Is.GreaterThan(p.xp.yMax),"Skill captions must clear the XP line.");
                Assert.That(p.potionTray.yMin,Is.GreaterThan(p.passives.Max(r=>r.yMax)));
                Assert.That(p.potionTray.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(p.potionTray.xMax,Is.LessThanOrEqualTo(p.width));
                foreach(var bottle in p.potions)
                {Assert.IsTrue(p.potionTray.Contains(bottle.min));Assert.IsTrue(p.potionTray.Contains(bottle.max));}
                foreach(var left in new[]{p.seal,p.level,p.hp,p.resource,p.shield,p.status,p.xpText})
                foreach(var right in p.actives.Concat(p.passives).Append(p.potionTray))
                    Assert.IsFalse(left.Overlaps(right),$"{width}x{height} at {factor}: {left} overlaps {right}");
            }
        }
        [Test] public void CooldownsRoundUpAndNeverFabricateReadyTime()
        {Assert.That(GlobalHudSnapshot.TimeLabel(0),Is.Empty);Assert.That(GlobalHudSnapshot.TimeLabel(3.21f),Is.EqualTo("3.3"));Assert.That(GlobalHudSnapshot.TimeLabel(10.01f),Is.EqualTo("11"));}
    }
}
