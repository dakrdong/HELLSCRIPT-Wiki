using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        SkillTreeD4Window guideModifierTree;
        int guideModifierNext;bool guideModifierReading;
        readonly RectTransform[] guideModifierFocus=new RectTransform[1];
        static readonly string[] GuideModifierPrompts={"피의 회수를 눌러 효과를 살펴보세요.","깊은 상처를 눌러 효과를 살펴보세요.","회전 베기를 눌러 효과를 살펴보세요."};
        static readonly string[] GuideModifierExplanations=
        {
            "피의 회수는 찢어낸 상처를 세 번 시전할 때마다 남은 출혈 피해를 한꺼번에 터뜨려요. 쌓인 출혈로 순간적인 피해를 주고 체력도 회복할 수 있어요.",
            "깊은 상처로 강화하면 여러 적 대신 한 명만 공격해요. 대신 출혈이 최대 세 번까지 중첩되고 중첩마다 더 강해져서, 한 적에게 큰 피해를 줄 수 있어요.",
            "회전 베기는 전방 공격을 360도 회전 공격으로 바꿔 주변의 적을 함께 베어요. 출혈 시간은 짧아지지만, 지금처럼 적들에게 둘러싸였을 때 효과적이에요.",
        };
        static int GuideRendIndex=>System.Array.FindIndex(SkillTreeD4.For(HeroClass.Warrior).actives,a=>a.id==GuideTutorial.Rend);
        void UnbindGuideModifierTree()
        {if(guideModifierTree!=null)guideModifierTree.SelectionAccepted-=GuideModifierSelected;guideModifierTree=null;}
        void ResetGuideModifierReview()
        {UnbindGuideModifierTree();guideModifierNext=0;guideModifierReading=false;guideModifierFocus[0]=null;}
        void BindGuideModifierTree(SkillTreeD4Window tree)
        {
            if(guideModifierTree==tree)return;UnbindGuideModifierTree();guideModifierTree=tree;
            if(tree!=null)tree.SelectionAccepted+=GuideModifierSelected;
        }
        void GuideModifierSelected(SkillTreeD4Selection selection)
        {
            if(guideLesson!="spin"||guideModifierNext>=3||guideModifierReading||guideModifierTree==null||guideModifierTree.Session.Hero!="Warrior")return;
            if(selection.Equals(SkillTreeD4Selection.ModifierOf(GuideRendIndex,guideModifierNext)))guideModifierReading=true;
        }
        void ConfirmGuideModifierRead()
        {if(guideLesson!="spin"||!guideModifierReading||guideModifierNext>=3)return;guideModifierReading=false;guideModifierNext++;}
        bool TickGuideModifierLesson()
        {
            if(guideLesson!="spin"||guideModifierNext>=3)return false;
            EnsureGate();
            if(CurrentSkillTree==null)ShowEdictSkillTree();
            var tree=CurrentSkillTree;BindGuideModifierTree(tree);
            if(tree==null){gate.Show(null,G,Loc.T("스킬창을 열어 주세요."));return true;}
            var wanted=SkillTreeD4Selection.ModifierOf(GuideRendIndex,guideModifierNext);
            if(guideModifierReading&&!tree.Session.Selection.Equals(wanted))guideModifierReading=false;
            ClearPrologueRing();
            if(guideModifierReading)
            {
                var description=tree.GuideDescriptionTarget(wanted);
                if(description!=null)
                {
                    string text=Loc.T(GuideModifierExplanations[guideModifierNext]);
                    guideModifierFocus[0]=description;gate.ShowMessage(G,text,ConfirmGuideModifierRead,description,tree.Panel,guideModifierFocus,emphasizeBright:true);return true;
                }
            }
            var target=tree.GuideModifierRowTarget(wanted)??tree.GuideTarget(wanted);
            if(target!=null&&UiVisibility.Fraction(target,UiSafeArea.Current)<.99f)UiVisibility.EnsureVisible(target);
            gate.Show(target,G,Loc.T(GuideModifierPrompts[guideModifierNext]),pointerAboveDim:true,strongGuidance:true);return true;
        }
    }
}
