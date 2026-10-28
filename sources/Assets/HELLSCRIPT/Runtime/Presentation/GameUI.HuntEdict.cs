using System;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        EdictPresetsWindow edictPresetsWindow;
        SkillTreeD4Window edictSkillTree;
        SkillTreeD4Window CurrentSkillTree=>edictSkillTree;
        public EdictPresetsWindow PresetsWindow=>edictPresetsWindow?.View!=null?edictPresetsWindow:null;
        public void ShowEdictShare()
        {
            if(game.TutorialActive&&!PrologueEdictOpen){ShowToast(Loc.T("목소리가 부를 때 사냥 칙령을 열 수 있습니다."));return;}
            if(BlockTrainingEdit())return;
            if(edictPresetsWindow?.View!=null)return;
            if(!EdictPresetsWindow.Available(game.Store.Data)){ShowToast(Loc.T("진행 후 공개됩니다."));return;}
            Tutorials.Discover(game.Store.Data,"H09");Tutorials.Discover(game.Store.Data,"H10");Tutorials.Discover(game.Store.Data,"H11");
            if(game.ComparisonRun&&game.Active){ShowComparisonConditions();return;}
            CloseHudPanel();
            var simulation=game.Active?game.Combat:null;

            var hero=comparisonEditing?comparisonHero:simulation?.Hero??game.Store.Data.Hero;
            if(comparisonEditing)hero.build=editing.Copy();
            string error="";
            bool Save(HuntEdictEditSession session)
            {
                if(comparisonEditing)
                {
                    try
                    {
                        HuntEdictLoadout.ValidateForHero(session.Draft,hero,game.catalog);
                        HuntEdictProgression.ValidateChange(game.Store.Data,hero,session.Draft,session.Recipes);
                        var candidate=session.Draft.ToBuild(editing);error=game.Comparison.ValidateB(candidate,comparisonEquipmentIds,session.Draft.edict);
                        if(error!="")return false;
                        GameStore.ApplyHuntEdict(hero,session.Draft);editing=hero.build.Copy();session.AcceptSaved(hero);return true;
                    }
                    catch(Exception e){error=e.Message;return false;}
                }
                if(simulation!=null&&simulation.State.training>=0&&!simulation.IsTutorial)
                {
                    try{HuntEdictProgression.ValidateChange(game.Store.Data,hero,session.Draft,session.Recipes);}catch(Exception e){error=e.Message;return false;}
                    if(!simulation.ApplyTrainingHuntEdict(session.Draft,out error))return false;
                    session.AcceptSaved(simulation.Hero);return true;
                }
                bool success=game.Store.CommitHuntEdict(session,game.catalog,simulation,true);error=game.Store.Error;return success;
            }
            edictPresetsWindow=EdictPresetsWindow.Open(transform,game.Store,game.catalog,hero,()=>InterfaceFactor,Save,()=>error,()=>
            {
                edictPresetsWindow=null;ClearEdictTutorialFocus();
                if(comparisonEditing)RenderBuild();
            },!comparisonEditing&&(simulation==null||simulation.State.training<0),simulation:simulation);
        }
        public void ShowEdictSkillTree()
        {
            if(game.TutorialActive&&!PrologueEdictOpen){ShowToast(Loc.T("목소리가 부를 때 사냥 칙령을 열 수 있습니다."));return;}
            if(BlockTrainingEdit()||CurrentSkillTree!=null)return;
            if(game.ComparisonRun&&game.Active){ShowComparisonConditions();return;}
            CloseHudPanel();var simulation=game.Active?game.Combat:null;
            var host=new StoreSkillTreeD4Host(game.Store,game.catalog,simulation,()=>!comparisonEditing&&(simulation==null||simulation.State.training<0||simulation.IsTutorial));
            edictSkillTree=SkillTreeD4Window.Open(transform,host,hero:SkillTreeD4.For(game.Store.Data.Hero.heroClass).hero,store:game.Store,initialSlot:game.TutorialActive&&!GuideActive?0:-1,closed:()=>
            {
                var previous=edictSkillTree;edictSkillTree=null;
                if(GuideSkillLesson)ReleaseGuideSkillTreeInput();
                ClearEdictTutorialFocus();
                if(previous!=null&&previous.CloseError!="")ShowToast(previous.CloseError);
            });
        }
        public void ShowEdictTab(string tab)
        {
            if(game.TutorialActive&&!PrologueEdictOpen){ShowToast(Loc.T("목소리가 부를 때 사냥 칙령을 열 수 있습니다."));return;}
            if(BlockTrainingEdit())return;
            if(quickRoot==null||quickKind!="edict"){CloseEdictQuick(true);ToggleEdictQuick();}
            if(quickRoot!=null&&sheetTab!=tab)OpenEdictSheet(tab);
        }
        void ShowRepeatSettingsLocked()=>ShowToast(RepeatHunt.SettingsLockedMessage,2);
        public void ShowEdictRepeat()=>ShowRepeatSettings();
    }
}
