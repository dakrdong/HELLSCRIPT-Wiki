using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        // Damage selection and its visible footprint read one source of radius/angle.
        internal (float radius,float angle) RakingWoundArea(ClassSkillCast cast)
            =>(N("W09","radius")*cast.area,cast.mod==3?360:N("W09","width"));
        bool ResolveD4WarriorNative(ClassSkillCast cast)
        {
            if(cast.id=="W17")
            {
                float duration=N(cast.id,"duration",cast:cast),period=N(cast.id,"period",1,cast);int ancestorCount=(int)N(cast.id,"summons",2,cast);
                for(int i=0;i<ancestorCount;i++)CS.effects.Add(new ClassSkillEffect{id="W17:ancestor:"+i,source=cast.id,kind="d4ancestor",mod=cast.mod,modRank=cast.modRank,root=cast.root,
                    created=State.time,until=State.time+duration,next=State.time+period,period=period,limit=Mathf.FloorToInt(duration/period),value=D4BaseValue(cast.id,0,cast)/100,
                    position=cast.origin,snapshot=cast.snapshot,copiedCast=cast.mod==2&&CS.lastWarriorActive?.root>0?JsonUtility.FromJson<ClassSkillCast>(JsonUtility.ToJson(CS.lastWarriorActive)):null});
                return true;
            }
            if(cast.id=="W18")
            {
                if(cast.mod==2)
                {var form=Buff("W18:form",D4ModValue(cast.id,cast),1.5f,cast.root,"form");form.source=cast.id;form.mod=cast.mod;form.modRank=cast.modRank;form.snapshot=cast.snapshot;return true;}
                int hits=0;foreach(var enemy in Nearby(State.position,N(cast.id,"radius",cast:cast)*cast.area,(int)N(cast.id,"targets",5,cast)))
                {if(SkillHit(cast,enemy,D4BaseValue(cast.id,0,cast)/100,0,modifierValue:true)!=null)hits++;ApplyStatus(enemy,StatusKind.Stun,cast.id,N(cast.id,"stunSeconds",cast:cast),cast.root);}
                if(hits>0&&Gear("DES_LW46"))Resource(20,cast.id,cast.root);
                return true;
            }
            if(cast.id=="W08")
            {
                var target=Enemy(cast.target);var victims=cast.mod==2?Line(cast.origin,cast.aim,N("W08","range",cast:cast),.7f*cast.area,(int)D4ModValue("W08",cast)):
                    target!=null&&Visible(target,State.position,N("W08","range",cast:cast))?new[]{target}:Array.Empty<EnemyState>();
                if(cast.mod==1){State.position=State.destination=cast.destination;Map.Repath();}
                int hits=0;foreach(var enemy in victims)
                {
                    if(SkillHit(cast,enemy,D4BaseValue("W08",0,cast)/100,0,modifierValue:true)==null)continue;hits++;
                    if(cast.mod==3)
                    {bool enhanced=Gear("DES_LW41")&&Proc("DES_LW41",4,cast.root);ApplyStatus(enemy,StatusKind.Mark,"W08:brand",enhanced?8:6,cast.root,-D4ModValue("W08",cast)/100);if(enhanced)Resource(8,"DES_LW41",cast.root);}
                }
                if(victims.Length>0&&cast.mod!=1)
                {
                    var pulled=victims[victims.Length-1];
                    Pull(pulled,State.position,N("W08","pullMeters"),"W08",cast.root);
                }
                if(hits>0)Resource(D4BaseValue("W08",1,cast)+(cast.mod==1?D4ModValue("W08",cast):0),"W08",cast.root);
                return true;
            }
            if(cast.id!="W09")return false;
            var rendArea=RakingWoundArea(cast);
            var targets=cast.mod==2?new[]{Enemy(cast.target)}.Where(e=>e!=null&&Visible(e,cast.origin,N(cast.id,"range",cast:cast))).ToArray():
                AreaTargets(cast.origin,rendArea.radius,cast.aim-cast.origin,rendArea.angle).Take((int)N(cast.id,"targets",5)).ToArray();
            var count=Counter("W09:casts");bool reclaim=cast.mod==1&&++count.count%3==0;int cashed=0;
            foreach(var enemy in targets)
            {
                if(reclaim&&CashDot("W09",enemy,1,cast,"W09")>0){cashed++;if(Gear("DES_LW42"))ApplyStatus(enemy,StatusKind.Stun,"DES_LW42",1,cast.root);}
                float coefficient=D4BaseValue("W09",0,cast)/100*(Gear("DES_LW42")?1.2f:1),duration=cast.mod==3?D4ModValue("W09",cast):N("W09","duration");
                if(cast.mod==2)
                {
                    var stacks=CS.dots.Where(d=>d.id=="W09"&&d.target==enemy.id).OrderBy(d=>d.expires).ToArray();
                    if(stacks.Length>=3){CS.dots.Remove(stacks[0]);stacks=stacks.Skip(1).ToArray();}
                    float increase=D4ModValue("W09",cast)/100;
                    Dot("W09",enemy,coefficient*(1+increase*(stacks.Length+1)),0,duration,cast,true,coefficient*(3+6*increase),true);
                }
                else Dot("W09",enemy,coefficient,0,duration,cast,modifierValue:true);
            }
            if(targets.Length>0)Resource(D4BaseValue("W09",1,cast),"W09",cast.root);
            if(cashed>0)Heal(cashed*D4ModValue("W09",cast)/100,"W09",cast.root);
            return true;
        }
        float D4WarriorNumber(string id,string key,float original,ClassSkillCast cast)
        {
            int mod=cast?.mod??Loadout.Modifier(id);float scale=cast?.rankScale??RankScale(id);
            if((id=="W08"||id=="W09")&&key=="range")return original*(cast?.formArea??D4FormArea);
            if(id=="W08"&&key=="resourceGain")return D4BaseValue(id,1,cast)/Mathf.Max(.0001f,scale);
            if(id=="W08"&&mod==1&&key=="prepare")return original+.25f;
            if(id=="W17"&&mod==1)
            {
                if(key=="summons")return 3;
                if(key=="duration")return D4ModValue(id,cast);
                if(key=="ticks")return Mathf.Floor(D4ModValue(id,cast)/Mathf.Max(.01f,Definition(id).Number("period",1)));
            }
            if(id=="W18"&&mod==1){if(key=="prepare")return 1.8f;if(key=="radius")return D4ModValue(id,cast);}
            return original;
        }
        float D4WarriorCost(string id,float original)
        {
            if(id=="W03")return D4BaseValue(id,1);if(id!="W01")return original;
            var cast=State.heroAction?.skill==0?Cast(State.heroAction.id):null;
            float cost=D4BaseValue(id,1,cast);return (cast?.mod??Loadout.Modifier(id))==3?cost*D4ModValue(id,cast):cost;
        }
        float D4WhirlInterval
        {
            get
            {
                if(!D4Combat)return .25f;var cast=State.heroAction?.skill==0?Cast(State.heroAction.id):null;
                return .25f*((cast?.mod??Loadout.Modifier("W01"))==3?D4ModValue("W01",cast):1);
            }
        }
        float D4WhirlSpeed(float speed)
        {
            if(!D4Combat||State.heroAction.phase!=HeroActionPhase.Channeling||State.heroAction.skill!=0)return speed;
            var cast=Cast(State.heroAction.id);return (cast?.mod??Loadout.Modifier("W01"))==1?speed*D4ModValue("W01",cast):speed;
        }
        void CaptureD4WarriorCast(ClassSkillCast cast)
        {
            if(cast.id=="W08"&&cast.mod==1)
            {
                Vector2 direction=(cast.aim-cast.origin).normalized;var stop=cast.aim-direction*.9f;
                cast.destination=Map.MoveDirect(cast.origin,stop,Mathf.Max(0,Vector2.Distance(cast.origin,stop)),.45f);
            }
        }
        void MoveD4WarriorCast(ClassSkillCast cast)
        {
            if(cast.id!="W08"||cast.mod!=1||State.time>=cast.releaseAt)return;
            float duration=cast.releaseAt-cast.started,progress=Mathf.Clamp01((State.time-cast.started)/Mathf.Max(.01f,duration));
            if(Map.CanLand(cast.destination)&&Map.LineClear(cast.origin,cast.destination)){State.position=Vector2.Lerp(cast.origin,cast.destination,progress);State.destination=State.position;}
        }
        void D4WarriorDeath(EnemyState enemy)
        {
            var mark=enemy.statuses.FirstOrDefault(s=>s.definitionId=="W08:brand"&&s.casterId==State.heroId&&s.remaining>0);
            if(mark!=null)Resource(20,"W08",mark.rootCastId);
        }
        bool ResolveD4WarriorLegacy(HeroActionState action)
        {
            string id=ClassSkills.LegacyId(action.skill);var cast=Cast(action.id);if(cast==null)return false;
            cast.snapshot=action.snapshot;
            if(id=="W02")
            {
                State.position=cast.destination;LandingPassives(action);RememberLeapLanding(action);
                int hits=AreaHit(cast.destination,RuneSkillRadius(1,2.5f)*cast.area,(float)SkillTreeD4Values.Active(id,0,1)/100,0,definition:id,root:cast.root);
                if(hits>0&&Stats.SetPieces("SWB")>=4){ItemEffects.crushCharge=4;EffectEvent("SWB4","CHARGE",root:cast.root,value:4);}
                if(Stats.specials.Contains("LW02"))AreaHit(cast.destination,4,Stats.AspectValue("LW02",.9f),0,default,360,false,"LW02",cast.root,DamageKind.Legendary);
                if(action.whirlwindReserved){AreaHit(cast.destination,3,1.8f,0,default,360,false,"SW4",cast.root,DamageKind.Set);action.whirlwindReserved=false;}
                if(Stats.Rune(RuneBonus.MobilityGuard)>0)ItemEffects.runeMobilityGuardUntil=State.time+2;
                if(cast.mod==2)D4WarriorBurst(cast,cast.destination,2.5f,D4ModValue(id,cast)/100,2,1,1.5f);
                return true;
            }
            if(id=="W03")
            {
                var victims=D4WarriorShape(id,cast.mod,cast.modRank,cast.origin,cast.aim,cast.area);
                float single=Stats.specials.Contains("LW03")&&victims.Length==1?Stats.AspectValue("LW03",.5f):0;
                bool killed=false;
                foreach(var enemy in victims)
                {
                    float execute=cast.mod==2&&AtHealthFraction(enemy.health,enemy.maxHealth,D4ModValue(id,cast)/100)?2:1;
                    Hit(enemy,(float)SkillTreeD4Values.Active(id,0,1)/100*execute,0,extraBonus:single+RuneMultiBonus(action.snapshot,victims.Length),snapshot:action.snapshot,definition:id,root:cast.root);
                    if(action.crushBonus)Hit(enemy,1.6f,0,false,definition:"SWB4",root:cast.root,kind:DamageKind.Set);
                    killed|=enemy.dead;
                }
                if(cast.mod==2&&killed&&Once("W03:refund",cast.root))Resource(cast.cost*.1f,id,cast.root);
                if(cast.mod==3)for(int i=0;i<2;i++)D4WarriorBurst(cast,cast.origin,3,D4ModValue(id,cast)/100,1);
                return true;
            }
            if(id=="W04")
            {
                var victims=D4WarriorShape(id,cast.mod,cast.modRank,cast.origin,cast.aim,cast.area);
                if(cast.mod==1)foreach(var enemy in victims)Pull(enemy,cast.origin,D4ModValue(id,cast),id,cast.root);
                foreach(var enemy in victims)
                {
                    float fraction=cast.mod==3?D4ModValue(id,cast)/100:1;
                    Hit(enemy,(float)SkillTreeD4Values.Active(id,0,1)/100*fraction,0,snapshot:action.snapshot,definition:id,root:cast.root);
                    if(cast.mod==2)ApplyStatus(enemy,StatusKind.Slow,id,3,cast.root,.35f);
                    else if(cast.mod!=3)ApplyStatus(enemy,StatusKind.Stun,id,D4BaseValue(id,1,cast),cast.root);
                }
                if(cast.mod==3)D4WarriorBurst(cast,cast.origin,3,D4BaseValue(id,0,cast)/100*D4ModValue(id,cast)/100,.6f,2,D4BaseValue(id,1,cast));
                return true;
            }
            if(id=="W05")
            {
                AddShield("W05",Stats.hp*D4BaseValue("W05",0,cast)/100,Set("REF_SW02",3)?5:4,cast.root);
                if(cast.mod==1){var stance=Buff("W05:stance",4,D4ModValue("W05",cast)/100,cast.root);stance.source="W05";stance.mod=cast.mod;stance.modRank=cast.modRank;stance.snapshot=CaptureDamage();}
                if(cast.mod==2){ClearHeroControl();var immune=Buff("W05:immune",D4ModValue("W05",cast),root:cast.root);immune.source="W05";immune.mod=cast.mod;immune.modRank=cast.modRank;}
                return true;
            }
            if(id=="W06")
            {
                if(cast.mod!=2)Resource(D4BaseValue("W06",0,cast)+(cast.mod==3?D4ModValue("W06",cast):0),"W06",cast.root);
                State.shoutTime=6;State.shoutBonus=D4BaseValue("W06",1,cast)/100;
                if(cast.mod>0)
                {
                    string suffix=cast.mod==1?"move":cast.mod==2?"haste":"regen-pause";
                    var buff=Buff("W06:"+suffix,6,cast.mod==3?0:D4ModValue("W06",cast)/100,cast.root);buff.source="W06";buff.mod=cast.mod;buff.modRank=cast.modRank;
                }
                return true;
            }
            return false;
        }
        EnemyState[] D4WarriorShape(string id,int mod,int modRank,Vector2 origin,Vector2 aim,float area=1)
        {
            float value=mod>0?(float)SkillTreeD4Values.Modifier(id+"-"+mod,modRank):0;
            if(id=="W04"&&mod==2)return Line(origin,aim,value*area,.8f*area,5);
            float radius=id=="W03"&&mod==1?value:id=="W04"&&mod==1?value:3;
            return AreaTargets(origin,RuneSkillRadius(id=="W03"?2:3,radius)*area,aim-origin,id=="W03"&&mod!=1?100:360);
        }
        void PrepareD4WarriorAction(HeroActionState action)
        {
            var cast=Cast(action.id);if(cast?.id!="W02"||cast.mod!=1)return;
            Vector2 direction=(cast.aim-cast.origin).normalized;
            cast.destination=Map.MoveDirect(cast.origin,cast.origin+direction*D4ModValue("W02",cast),D4ModValue("W02",cast),.45f);
            action.destination=cast.destination;
        }
        void PushD4ChargePath(HeroActionState action)
        {
            var cast=Cast(action.id);var forward=(cast.destination-cast.origin).normalized;var side=new Vector2(-forward.y,forward.x);
            foreach(var enemy in Line(cast.origin,State.position,Vector2.Distance(cast.origin,State.position)+.5f,.8f,100))
            {
                if(!Once("W02:push",cast.root,enemy.id))continue;
                float sign=Vector2.Dot(enemy.position-cast.origin,side);if(Mathf.Abs(sign)<.01f)sign=enemy.id%2==0?1:-1;
                Pull(enemy,enemy.position+side*Mathf.Sign(sign)*1.5f,1.5f,"W02",cast.root);
            }
        }
        void SyncD4LeapCharges()
        {
            if(Loadout.Modifier("W02")!=3||!Loadout.Equipped("W02"))return;
            CS.charges??=new System.Collections.Generic.List<ClassSkillCharge>();var charge=CS.charges.FirstOrDefault(c=>c.id=="W02");
            if(charge==null){charge=new ClassSkillCharge{id="W02"};CS.charges.Add(charge);}
            while(charge.count<2&&State.time+.00001f>=charge.next){charge.count++;charge.next=charge.count<2?charge.next+charge.duration:0;}
            State.cooldowns[1]=charge.count>0?0:Mathf.Max(0,charge.next-State.time);State.cooldownTotals[1]=charge.duration;
        }
        void ConsumeD4LeapCharge(ClassSkillCast cast)
        {
            SyncD4LeapCharges();var charge=CS.charges.Single(c=>c.id=="W02");
            if(charge.count==2){charge.duration=D4ModValue("W02",cast);charge.next=State.time+charge.duration;}
            charge.count=Mathf.Max(0,charge.count-1);SyncD4LeapCharges();
        }
        void D4WarriorBurst(ClassSkillCast cast,Vector2 center,float radius,float coefficient,float delay,int count=1,float finalStun=0)
        {
            CS.effects.Add(new ClassSkillEffect{id=cast.id+":burst:"+State.nextId++,source=cast.id,mod=cast.mod,modRank=cast.modRank,kind="d4burst",root=cast.root,
                position=center,radius=radius*cast.area,value=coefficient,extra=finalStun,next=State.time+delay,until=State.time+delay*count,period=delay,limit=count,snapshot=cast.snapshot});
        }
        bool TickD4WarriorEffect(ClassSkillEffect effect,ClassSkillCast cast)
        {
            if(effect.kind=="d4ancestor")
            {
                effect.position=State.position;
                while(effect.next<=State.time+.00001f&&effect.next<=effect.until+.00001f&&effect.count<effect.limit)
                {
                    effect.next+=effect.period;effect.count++;
                    if(effect.mod==2&&(effect.copiedCast?.id=="W05"||effect.copiedCast?.id=="W06"))
                    {CopyD4WarriorSupport(cast,effect.copiedCast);continue;}
                    var target=Nearby(State.position,6,1).FirstOrDefault();if(target==null)continue;
                    if(effect.mod==2&&effect.copiedCast?.root>0)
                    {
                        var copied=effect.copiedCast;float power=D4BaseValue(copied.id,0,copied)/100*D4ModValue("W17",cast)/100;
                        var victims=copied.id=="W03"||copied.id=="W04"?D4WarriorShape(copied.id,copied.mod,copied.modRank,State.position,target.position,copied.area):
                            copied.id=="W08"?new[]{target}:AreaTargets(State.position,(copied.id=="W01"?2.5f*(copied.mod==2?D4ModValue(copied.id,copied):1):copied.id=="W02"?2.5f:3)*copied.area,target.position-State.position,copied.id=="W09"&&copied.mod!=3?90:360);
                        foreach(var enemy in victims.Take(5))SkillHit(cast,enemy,power,0,true,"W17",modifierValue:true);
                    }
                    else SkillHit(cast,target,effect.value,0,true,"W17",modifierValue:true);
                    if(Gear("DES_LW46")&&Once("DES_LW46",cast.root))Buff("DES_LW46",4,.2f,cast.root);
                }
                return true;
            }
            if(effect.kind!="d4burst")return false;
            while(effect.next<=State.time+.00001f&&effect.count<effect.limit)
            {
                effect.next+=effect.period;effect.count++;
                foreach(var enemy in Nearby(effect.position,effect.radius,5))
                {
                    SkillHit(cast,enemy,effect.value,0,true,effect.source,modifierValue:true);
                    if(effect.count==effect.limit&&effect.extra>0)ApplyStatus(enemy,StatusKind.Stun,effect.source,effect.extra,effect.root);
                }
            }
            return true;
        }
        ClassSkillEffect D4CopiedSupportEffect(ClassSkillCast ancestor,string suffix,float duration,float value=0)
        {
            var effect=Buff("W17:"+suffix,duration,value,ancestor.root);effect.source="W17";effect.mod=ancestor.mod;effect.modRank=ancestor.modRank;effect.snapshot=ancestor.snapshot;return effect;
        }
        void CopyD4WarriorSupport(ClassSkillCast ancestor,ClassSkillCast copied)
        {
            float ratio=D4ModValue("W17",ancestor)/100;
            if(copied.id=="W05")
            {
                AddShield("W17:shield",Stats.hp*D4BaseValue("W05",0,copied)/100*ratio,Set("REF_SW02",3)?5:4,ancestor.root);
                if(copied.mod==1)D4CopiedSupportEffect(ancestor,"stance",4,D4ModValue("W05",copied)/100*ratio);
                if(copied.mod==2){ClearHeroControl();D4CopiedSupportEffect(ancestor,"immune",D4ModValue("W05",copied)*ratio);}
                if(copied.mod==3)D4CopiedSupportEffect(ancestor,"reflection",4,D4ModValue("W05",copied)/100*ratio);
                return;
            }
            if(copied.mod!=2)Resource((D4BaseValue("W06",0,copied)+(copied.mod==3?D4ModValue("W06",copied):0))*ratio,"W17",ancestor.root);
            D4CopiedSupportEffect(ancestor,"shout",6,D4BaseValue("W06",1,copied)/100*ratio);
            if(copied.mod>0)D4CopiedSupportEffect(ancestor,copied.mod==1?"move":copied.mod==2?"haste":"regen-pause",6,copied.mod==3?0:D4ModValue("W06",copied)/100*ratio);
        }
        void CaptureD4WarriorPassives(ClassSkillCast cast)
        {
            cast.formArea=Fx("W18:form")!=null?1.5f:1;cast.area*=cast.formArea;
            if(!Normal(cast.id)||!Passive("WP21"))return;
            string role=SkillTreeD4.For(Hero.heroClass).actives.Single(a=>a.id==cast.id).roleKo.Split('\u00b7')[0].Trim();var history=Counter("WP21");
            bool active=history.until>State.time;
            history.count=active&&history.previous!=role?Math.Min(3,history.count+1):active?history.count:0;
            history.previous=role;history.until=State.time+4;cast.additive+=history.count*PV("WP21");
        }
        void D4WarriorHurt(EnemyState attacker,bool blocked)
        {
            if(!blocked||attacker==null)return;
            foreach(var id in new[]{"W05","W17"})
            {
                var stance=Fx(id+":stance");bool shield=id=="W05"?OwnIronWall:State.shields.Any(s=>s.definitionId=="W17:shield"&&s.amount>0);
                if(stance==null||stance.count>=2||!shield||!Interval(id+":counter",1))continue;
                var cast=Cast(stance.root);if(cast==null)continue;stance.count++;
                foreach(var enemy in AreaTargets(State.position,3,attacker.position-State.position,90).Take(5))SkillHit(cast,enemy,stance.value,0,true,id,modifierValue:true);
            }
        }
        bool ReflectD4IronWall(ShieldEffect shield,float absorbed)
        {
            var cast=Cast(shield.rootCastId);var reflection=Fx("W17:reflection");bool copied=shield.definitionId=="W17:shield"&&reflection?.root==shield.rootCastId;
            if(cast==null||absorbed<=0||!copied&&(shield.definitionId!="W05"||cast.mod!=3))return false;
            float amount=absorbed*(copied?reflection.value:D4ModValue("W05",cast)/100);
            foreach(var enemy in Nearby(State.position,3,5))ApplyOutgoing(enemy,amount,0,1,1,false,0,cast.snapshot??CaptureDamage(),copied?"EXTRA:W17":"EXTRA:W05",cast.root,0,DamageKind.Set,attackResolved:true);
            return true;
        }
    }
}
