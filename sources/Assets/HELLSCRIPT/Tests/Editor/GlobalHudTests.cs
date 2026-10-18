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
            label.font=UiFonts.CasualBody(true);label.text="보호막 +25";
            label.horizontalOverflow=HorizontalWrapMode.Overflow;label.verticalOverflow=VerticalWrapMode.Overflow;
            try
            {
                foreach(var size in new[]{new Vector2Int(360,640),new Vector2Int(640,360),new Vector2Int(900,360),new Vector2Int(540,960),new Vector2Int(720,1280),new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(900,1600),new Vector2Int(3440,1440)})
                {
                    var p=new GlobalHudLayout(size.x,size.y,1f,style);
                    string where=" at "+size.x+"x"+size.y;label.text="\uBCF4\uD638\uB9C9 +25";
                    foreach(var row in new[]{("Level",p.level,style.levelFont,style.minimumLevelFont),("HP",p.hp,style.valueFont,style.minimumValueFont),
                        ("Resource",p.resource,style.valueFont,style.minimumValueFont),("Shield",p.shield,style.shieldFont,style.minimumFont),("XP",p.xpText,style.captionFont,style.minimumFont)})
                    {
                        label.font=row.Item1=="Level"?UiFonts.CasualDisplay:UiFonts.CasualBody(true);
                        label.fontSize=p.FontSize(row.Item3,(int)row.Item4);
                        Assert.That(label.fontSize/p.textScale,Is.EqualTo(Mathf.Max(row.Item3,row.Item4)).Within(.5f/p.textScale+.001f),row.Item1+" text no longer proportional"+where);
                        Assert.That(label.preferredHeight,Is.LessThanOrEqualTo(row.Item2.height*p.scale+1),row.Item1+" text clipped"+where);
                    }
                    label.font=UiFonts.CasualBody(true);label.fontSize=p.FontSize(style.statusFont,(int)style.minimumFont);
                    foreach(string text in new[]{"\uC0C1\uC2DC","99","\u221e"})
                    {
                        label.text=text;
                        Assert.That(label.preferredHeight,Is.LessThanOrEqualTo((p.status.height-p.statusIcon)*p.scale+1),"Status caption clipped"+where);
                    }
                    Assert.IsFalse(p.shield.Overlaps(p.status),"Shield band runs into the status strip"+where);
                    Assert.That(p.shield.y,Is.GreaterThanOrEqualTo(p.shieldLine.yMax),"Shield text sits on its own line"+where);
                }
            }
            finally{Object.DestroyImmediate(host);if(created&&font!=null)Object.DestroyImmediate(font);}
        }
        // Legacy gear and content shortcuts prefer the skill icons' size plus the emblem margin, while
        // six controls and their rail spacing must fit above the HUD without dropping below the minimum.
        [Test] public void ShortcutsMatchTheSkillIconsAndKeepTheirMinimum()
        {
            var style=GlobalHudStyle.Load();
            foreach(var size in new[]{new Vector2Int(854,450),new Vector2Int(956,440),new Vector2Int(1600,900),new Vector2Int(1600,1000),new Vector2Int(2100,900),new Vector2Int(440,956),new Vector2Int(360,780)})
            {
                var hud=new GlobalHudLayout(size.x,size.y,1f,style);float canvas=UiTheme.Scale(new Rect(0,0,size.x,size.y))*.5f;
                float hudTop=(size.y-hud.potionBounds.yMax*hud.scale)/canvas,skillWidth=hud.actives[0].width,skill=skillWidth*hud.scale;string where=" at "+size.x+"x"+size.y;
                float town=GlobalHudLayout.ShortcutIcon(skillWidth,hud.scale,canvas,hudTop,44/canvas),battle=GlobalHudLayout.ShortcutIcon(skillWidth,hud.scale,canvas,hudTop,44);
                Assert.That(town*canvas,Is.GreaterThanOrEqualTo(Mathf.Min(44f,skill)-.01f),"Town shortcut smaller than its pointer target or a skill icon"+where);
                Assert.That(battle,Is.GreaterThanOrEqualTo(44f-.01f),"Battle shortcuts shrank below the 44 page units they always had"+where);
                float railSpacing=(3*12+5*UiTheme.Gap)*hud.scale;
                Assert.That(6*town*canvas+railSpacing,Is.LessThanOrEqualTo(hudTop*canvas+.01f),"Six town shortcuts and their rail spacing do not fit above the HUD"+where);
                Assert.That(6*battle*canvas+railSpacing,Is.LessThanOrEqualTo(hudTop*canvas+.01f),"Six battle shortcuts and their rail spacing do not fit above the HUD"+where);
                if(size.x>size.y&&size.x<=1600&&size.y<=900&&battle*canvas>44.01f&&6*skill*1.15f+railSpacing<=hudTop*canvas)
                    Assert.That(battle*canvas,Is.EqualTo(skill*1.15f).Within(.01f),"A landscape battle shortcut with enough rail space is not the skill icon's size"+where);
            }
            Assert.That(GlobalHudLayout.ShortcutIcon(76.8f,1,1,731,44),Is.EqualTo(88.32f).Within(.01f));
            Assert.That(GlobalHudLayout.ShortcutIcon(76.8f,.5f,.5f,731,44),Is.EqualTo(88.32f).Within(.01f),"The size must not depend on the window size in landscape");
            Assert.That(GlobalHudLayout.ShortcutIcon(76.8f,1,1,200,44),Is.LessThan(88.32f),"Six shortcuts must fit even in a short window");
            Assert.That(GlobalHudLayout.ShortcutIcon(76.8f,.275f,.543f,1705,44),Is.EqualTo(44.7f).Within(.1f),"A small portrait window keeps the shortcuts at about the 44 page units they always had in battle");
        }
        [Test] public void LandscapeLocksCasualReferenceGeometry()
        {
            var p=new GlobalHudLayout(1600,900);
            Assert.That(p.scale,Is.EqualTo(1));Assert.That(p.potions.Length,Is.EqualTo(3));Assert.That(p.actives.Length,Is.EqualTo(4));
            Assert.That(p.actives.All(r=>Mathf.Abs(r.width-76)<.001f),Is.True);
            Assert.That(p.potions.All(r=>Mathf.Abs(r.width-76)<.001f&&Mathf.Abs(r.center.y-p.actives[0].center.y)<.001f),Is.True);
            Assert.That(p.ultimate.x-p.potions[2].xMax,Is.EqualTo(24).Within(.001));
            Assert.That(p.status.width,Is.EqualTo(230));Assert.That(p.maximumStatusWidth,Is.EqualTo(408));
            Assert.That(p.xp,Is.EqualTo(new Rect(32,12,1536,8)));
            Assert.That(p.Pixels(p.edict),Is.EqualTo(new Rect(1516,766,60,60)));
        }
        [TestCase(440,956,46)] [TestCase(956,440,48)] [TestCase(1600,900,76)]
        public void OrientationOwnsItsCompositionAndUsesOneScale(int width,int height,int slot)
        {
            var p=new GlobalHudLayout(width,height);Assert.That(p.Pixels(p.actives[0]).width,Is.EqualTo(slot).Within(.001));
            Assert.That(p.wrapped,Is.EqualTo(width<height));
            Assert.That(p.potions.Append(p.ultimate).All(r=>r.y==p.actives[0].y),Is.True);
            if(width<height){Assert.Greater(p.status.yMin,p.actives[0].yMax);Assert.Greater(p.actives[0].yMin,p.seal.yMax);Assert.Greater(p.seal.yMin,p.xp.yMax);}
            else Assert.That(p.status.xMin,Is.EqualTo(p.potions[0].xMin).Within(.001));
            Assert.That(p.Pixels(p.edict).yMin,Is.GreaterThan(height*.7f));
        }
        [Test] public void ProportionalWindowChangesDoNotMultiplyFontScaleOnRefresh()
        {
            var large=new GlobalHudLayout(1600,900);var small=new GlobalHudLayout(800,450);
            Assert.That(small.Pixels(small.actives[0]).width/large.Pixels(large.actives[0]).width,Is.EqualTo(.5f).Within(.001));
            foreach(float font in new[]{12f,16f,20f,22f})for(int refresh=0;refresh<30;refresh++)
                Assert.That(new GlobalHudLayout(800,450).FontSize(font,10),Is.EqualTo(large.FontSize(font,10)/2));
        }
        [TestCase(640,360)] [TestCase(360,640)] [TestCase(1200,900)] [TestCase(2000,900)]
        public void AllFiveSkillsAndThreePotionsStayInside(int w,int h)
        {
            var p=new GlobalHudLayout(w,h);
            var slots=p.actives.Append(p.ultimate).Concat(p.potions).ToArray();
            foreach(var r in slots){Assert.GreaterOrEqual(r.xMin,0);Assert.GreaterOrEqual(r.yMin,0);Assert.LessOrEqual(r.xMax,p.width+.01f);Assert.LessOrEqual(r.yMax,p.height+.01f);}
            for(int i=0;i<slots.Length;i++)for(int j=i+1;j<slots.Length;j++)Assert.IsFalse(slots[i].Overlaps(slots[j]));
            foreach(var potion in p.potions)foreach(var skill in p.actives.Append(p.ultimate))Assert.Less(potion.xMax,skill.xMin);
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
        public void BottomAnchoredActionsAndPotionsDoNotOverlapVitals(int width,int height)
        {
            var style=GlobalHudStyle.Load();
            { const float factor=1f;
                var p=new GlobalHudLayout(width,height,factor,style);
                float baseline=(!p.landscape?124:p.compact?37:54)*p.presentationScale;
                Assert.That(p.Pixels(p.actives[0]).yMin,Is.EqualTo(baseline).Within(.001),"The face must reserve its caption above the XP line.");
                if(p.landscape)Assert.That(p.actives[0].yMin-style.captionHeight,Is.GreaterThan(p.xp.yMax),"Skill captions must clear the XP line.");
                Assert.That(p.potionBounds.xMax,Is.LessThan(p.ultimate.xMin));
                Assert.That(p.potions[0].center.y,Is.EqualTo(p.actives[0].center.y).Within(.001));
                Assert.That(p.potionBounds.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(p.potionBounds.xMax,Is.LessThanOrEqualTo(p.width));
                foreach(var bottle in p.potions)
                {Assert.That(bottle.xMin,Is.GreaterThanOrEqualTo(p.potionBounds.xMin-.001f));Assert.That(bottle.xMax,Is.LessThanOrEqualTo(p.potionBounds.xMax+.001f));Assert.That(bottle.yMax,Is.LessThanOrEqualTo(p.potionBounds.yMax+.001f));}
                foreach(var left in new[]{p.seal,p.level,p.hp,p.resource,p.shield,p.status,p.xpText})
                foreach(var right in p.actives.Append(p.ultimate).Append(p.potionBounds))
                    Assert.IsFalse(left.Overlaps(right),$"{width}x{height} at {factor}: {left} overlaps {right}");
            }
        }
        [Test] public void CooldownsRoundUpAndNeverFabricateReadyTime()
        {Assert.That(GlobalHudSnapshot.TimeLabel(0),Is.Empty);Assert.That(GlobalHudSnapshot.TimeLabel(3.21f),Is.EqualTo("3.3"));Assert.That(GlobalHudSnapshot.TimeLabel(10.01f),Is.EqualTo("11"));}
        [Test] public void EquippedPassivesRemainInspectableWithoutEmptyOrLockedPlaceholders()
        {
            var snapshot=GlobalHudSnapshot.Sample(2);
            snapshot.passives[0].remaining=2.5f;snapshot.passives[0].total=4;
            snapshot.passives[1].empty=true;snapshot.passives[2].locked=true;
            string before=JsonUtility.ToJson(snapshot);
            var visible=snapshot.ObservationEffects();
            Assert.That(visible.Count,Is.EqualTo(3));
            var passive=visible.Single(e=>e.key=="passive:"+LegacySkillWire.SlotId(HeroClass.Mage,1,true));
            Assert.That(passive.remaining,Is.EqualTo(2.5f));Assert.That(passive.total,Is.EqualTo(4));Assert.IsFalse(passive.permanent);
            Assert.That(passive.name,Is.EqualTo(snapshot.passives[0].name));
            Assert.That(JsonUtility.ToJson(snapshot),Is.EqualTo(before),"Projecting the HUD changed its source snapshot");
            snapshot.passives[0].remaining=0;
            Assert.IsTrue(snapshot.ObservationEffects().Last().permanent);
        }
    }
}
