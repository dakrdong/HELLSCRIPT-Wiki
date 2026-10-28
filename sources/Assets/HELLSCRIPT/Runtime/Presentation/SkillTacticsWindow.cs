using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hellscript
{
    // The shared shell owns geometry/input; the detached session uses the existing store transaction.
    public sealed partial class SkillTacticsWindow
    {
        readonly GameStore store;readonly GameCatalog catalog;
        readonly CombatSimulation simulation;
        readonly ClassSkillLoadout opened;readonly int level;
        readonly UiSkin skin=UiSkin.Casual;
        Action<SkillTacticsLeaveTarget> closed;
        bool closing,building,focusModifier;
        float scrollOffset;
        string result="";bool failed;
        GameObject selectedControl;string selectedName;bool selectedWithKeyboard;
        ContentWindowView dialog;
        Text status,changed,summary;Button saveButton,revertButton;
        RectTransform optionsBody;ScrollRect optionsScroll;
        public ContentWindowView View {get;private set;}
        public SkillTacticsEditSession Session {get;}
        public bool HasDialog=>dialog!=null;
        public bool IsOpen=>!closing&&View!=null;
        public int LayoutBuilds {get;private set;}
        public ScrollRect Scroll=>optionsScroll;
        public static bool DevelopmentHostAvailable
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            get=>true;
#else
            get=>false;
#endif
        }
        SkillTacticsWindow(GameStore store,GameCatalog catalog,string node,SkillTacticsReturnState from,Action<SkillTacticsLeaveTarget> closed,SkillTacticsHostKind host,CombatSimulation simulation)
        {
            this.store=store;this.catalog=catalog;this.closed=closed;this.simulation=simulation;
            Session=new SkillTacticsEditSession(store.Data.Hero,node,host,from);opened=Session.Draft;level=store.Data.Hero.level;
            focusModifier=node!=Session.Card.skill;
        }
        public static SkillTacticsWindow OpenDevelopment(Transform parent,GameStore store,GameCatalog catalog,string node,
            Func<Rect,(float scale,Rect plate)> fit,Action<SkillTacticsLeaveTarget> closed,SkillTacticsReturnState from=null)
        {
            if(!DevelopmentHostAvailable)
                throw new InvalidOperationException("Tactical screens are available only in an explicit development host.");
            return Open(parent,store,catalog,node,fit,closed,from,SkillTacticsHostKind.Development);
        }
        public static SkillTacticsWindow Open(Transform parent,GameStore store,GameCatalog catalog,string node,
            Func<Rect,(float scale,Rect plate)> fit,Action<SkillTacticsLeaveTarget> closed,SkillTacticsReturnState from=null,SkillTacticsHostKind host=SkillTacticsHostKind.Player,CombatSimulation simulation=null)
        {
            if(!SkillTacticsCatalog.CanEnter(host,node))throw new InvalidOperationException("Tactical editing is unavailable for this host or node.");
            if(store==null||catalog==null)throw new ArgumentNullException(store==null?nameof(store):nameof(catalog));
            if(store.Data.Hero.build.classSkills?.version!=5)throw new NotSupportedException("Historical loadouts remain in their original editor.");
            if(fit==null)throw new ArgumentNullException(nameof(fit));
            var window=new SkillTacticsWindow(store,catalog,node,from,closed,host,simulation);
            window.View=ContentWindowView.Open(parent,"스킬별 사냥칙령",EquipmentViewSource.Draft,window.Reading,window.Render,
                back:()=>window.RequestLeave(SkillTacticsLeaveTarget.SkillTree),skin:UiSkin.Casual,fit:fit);
            SkillTacticsViewLifetime.Watch(window.View,window.UnexpectedDestroy);
            StoreViewBinding.Attach(window.View,store,window.View.Repaint,()=>!window.HasDialog,
                visibleState:()=>store.Data.Hero.id+"|"+store.Data.Hero.useRecommendedEdict);
            return window;
        }
        public bool SetValue(string key,string value)=>Edit(()=>Session.Set(key,value,level));
        public bool Step(string key,int direction)
            =>IsOpen&&!HasDialog&&(direction==-1||direction==1)&&Session.CanStep(key,direction)&&Edit(()=>Session.StepNumber(key,direction,level));
        bool Edit(Action edit)
        {
            if(!IsOpen||HasDialog)return false;
            Session.RefreshEligibility(store.Data.Hero);
            try{if(Session.Preview&&!Session.CanEditBasic)throw new InvalidOperationException(Session.ReadOnlyReason);edit();result="";failed=false;RefreshValues();return true;}
            catch(Exception error){result=Explain(error.Message);failed=true;RefreshValues();return false;}
        }
        public bool Save()
        {
            if(!IsOpen)return false;
            Session.RefreshEligibility(store.Data.Hero);
            if(!Session.CanCommit){failed=true;result=Explain(Session.ReadOnlyReason);RefreshValues();return false;}
            bool ok=store.CommitSkillTactics(Session,catalog,simulation,true);failed=!ok;result=ok?Loc.T("저장했습니다."):Explain(Session.Error);
            RefreshValues();return ok;
        }
        public void Revert()
        {if(!IsOpen||HasDialog)return;Session.Revert();result="";failed=false;View.Repaint();}
        string Explain(string value)
        {
            var reason=SkillTacticsCatalog.Data.reasons.FirstOrDefault(r=>r.label.ko==value||r.label.en==value);
            return reason?.label.Text??Loc.T(value??"");
        }
        float Reading()
        {
            var current=EventSystem.current?.currentSelectedGameObject;
            if(current!=null&&current!=selectedControl&&View!=null&&current.activeInHierarchy&&current.transform.IsChildOf(View.transform))
            {selectedControl=current;selectedName=current.name;var button=current.GetComponent<UiButton>();selectedWithKeyboard=button==null||button.KeyboardFocused;}
            return 1;
        }
        void RestoreFocus()
        {
            var events=EventSystem.current;if(events==null||selectedName==null)return;
            var host=View.GetComponentInParent<ContentWindowHost>();if(host!=null&&host.TopOwner!=View)return;
            var control=View.GetComponentsInChildren<Selectable>().FirstOrDefault(c=>c.name==selectedName&&c.IsActive()&&c.IsInteractable());
            if(control!=null){selectedControl=control.gameObject;events.SetSelectedGameObject(selectedControl,selectedWithKeyboard?new BaseEventData(events):new PointerEventData(events));}
        }
        string Name(string id)
        {
            if(string.IsNullOrEmpty(id))return Loc.T("없음");
            var data=SkillTreeD4.Model.Classes[((HeroClass)opened.heroClass).ToString()];
            foreach(var active in data.Actives)
            {
                if(active.Id==id)return active.Name[Loc.Language];
                var modifier=active.Modifiers.FirstOrDefault(m=>m.Id==id);if(modifier!=null)return modifier.Name[Loc.Language];
            }
            return Loc.T("알 수 없는 스킬");
        }
        string AppliedModifier()
        {
            int index=opened.Modifier(Session.Card.skill)-1;
            var active=SkillTreeD4.Model.Classes[((HeroClass)opened.heroClass).ToString()].Actives.Single(a=>a.Id==Session.Card.skill);
            return index>=0?active.Modifiers[index].Id:"";
        }
        void ReleaseMeasurement()
        {
            (measurement as IDisposable)?.Dispose();measurement=null;
        }
        static void Dispose(Component view)
        {
            if(view==null)return;ContentWindowHost.Detach(view);view.gameObject.SetActive(false);
            if(Application.isPlaying)UnityEngine.Object.Destroy(view.gameObject);else UnityEngine.Object.DestroyImmediate(view.gameObject);
        }
        void End(SkillTacticsLeaveTarget target)
        {
            if(!IsOpen)return;closing=true;var done=closed;closed=null;DismissDialog();Dispose(View);ReleaseMeasurement();done?.Invoke(target);
        }
        // A destroyed owner cannot offer a navigation decision. Cancel the detached draft without a write or close callback.
        public void OwnerDestroyed()
        {
            if(closing)return;closing=true;closed=null;DismissDialog();
            Dispose(View);ReleaseMeasurement();
        }
        void UnexpectedDestroy()
        {
            if(closing)return;closing=true;DismissDialog();ReleaseMeasurement();var done=closed;closed=null;done?.Invoke(SkillTacticsLeaveTarget.SkillTree);
        }

    }
    // Explicit development previews need the same cancellation when deleted outside Play Mode.
    [ExecuteAlways]
    public sealed class SkillTacticsViewLifetime:MonoBehaviour
    {
        Action destroyed;ContentWindowHost host;Component owner;
        internal static void Watch(Component owner,Action destroyed)
        {
            var lifetime=owner.gameObject.AddComponent<SkillTacticsViewLifetime>();
            lifetime.host=owner.GetComponentInParent<ContentWindowHost>();lifetime.owner=owner;lifetime.destroyed=destroyed;
        }
        void OnDestroy()
        {
            // Capture the host while the component is alive; the owner's native object may already be gone here.
            if(host!=null)host.Remove(owner);
            var callback=destroyed;destroyed=null;callback?.Invoke();
        }
    }
}
