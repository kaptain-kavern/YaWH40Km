using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
namespace Corruption
{
    internal class Bookshelf : Building_Storage
    {
        public Pawn pawn;
        public List<ThingDef> StoredBooks = new List<ThingDef>();
        public int StoriesCount = 3;
        public Thing ChoosenBook = null;
        public List<ThingDef> MissingBooksList = new List<ThingDef>();
        private Graphic MiniGraphic;
        public override Graphic Graphic
        {
            get
            {
                Graphic result;
                if (MiniGraphic != null && !Spawned)
                {
                    result = MiniGraphic;
                }
                else
                {
                    result = base.Graphic;
                }
                return result;
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.LookList<ThingDef>(ref StoredBooks, "StoredBooks", LookMode.DefReference, null);
            Scribe_Collections.LookList<ThingDef>(ref MissingBooksList, "MissingBooksList", LookMode.DefReference, null);
        }
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            if (StoredBooks.Count <= 0 && MissingBooksList.Count <= 0)
            {
                ReadXML();
                MiniGraphic = GraphicDatabase.Get<Graphic_Multi>("Clutter/Furniture/BookShelf/MiniBookShelf");
                MiniGraphic.drawSize = def.graphicData.drawSize;
            }
        }
        public Thing JobBook(Pawn reader)
        {
            if (StoredBooks.Count <= 0)
            {
                reader.jobs.StopAll();
            }
            else
            {
                ThingDef thingDef = StoredBooks.RandomElement<ThingDef>();
                StoredBooks.Remove(thingDef);
                MissingBooksList.Add(thingDef);
                ChoosenBook = ThingMaker.MakeThing(thingDef);
            }
            return ChoosenBook;
        }
        private void ReadXML()
        {
            ThingDef_Readables clutterThingDefs = (ThingDef_Readables)def;
            if (clutterThingDefs.BooksList.Count > 0 && StoredBooks.Count <= 0)
            {
                while (StoredBooks.Count < 3 && MissingBooksList.Count <= 0)
                {
                    Thing thing = ThingMaker.MakeThing(clutterThingDefs.BooksList.RandomElement<ThingDef>());
                    if (!StoredBooks.Contains(thing.def))
                    {
                        StoredBooks.Add(thing.def);
                    }
                }
            }
        }
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            IEnumerable<FloatMenuOption> result;
            if (!myPawn.CanReserve(this))
            {
                FloatMenuOption item = new FloatMenuOption("CannotUseReserved".Translate(), null);
                result = new List<FloatMenuOption>
                {
                    item
                };
            }
            else
            {
                if (!myPawn.CanReach(this, PathEndMode.Touch, Danger.Some))
                {
                    FloatMenuOption item2 = new FloatMenuOption("CannotUseNoPath".Translate(), null);
                    result = new List<FloatMenuOption>
                    {
                        item2
                    };
                }
                else
                {
                    if (StoredBooks.Count <= 0)
                    {
                        FloatMenuOption item3 = new FloatMenuOption("ClutterNoStoryBooks".Translate(), null);
                        result = new List<FloatMenuOption>
                        {
                            item3
                        };
                    }
                    else
                    {
                        Action action = delegate
                        {
                            Job newJob = new Job(DefDatabase<JobDef>.GetNamed("ClutterSitAndRead"), this);
                            myPawn.QueueJob(newJob);
                            myPawn.jobs.StopAll();
                            pawn = myPawn;
                            myPawn.Reserve(this);
                        };
                        list.Add(new FloatMenuOption("ClutterReadABook".Translate(), action));
                        result = list;
                    }
                }
            }
            return result;
        }
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            stringBuilder.Append("ClutterStringBookAmmount".Translate());
            if (StoredBooks.Count > 0)
            {
                stringBuilder.Append(StoredBooks.Count);
                for (int i = 0; i < StoredBooks.Count; i++)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.Append(i + 1 + ". " + StoredBooks.ElementAt(i).LabelCap);
                }
            }
            else
            {
                stringBuilder.Append("ClutterStringNoStoryBooks".Translate());
            }
            return stringBuilder.ToString();
        }
        public override void DeSpawn()
        {
            if (MissingBooksList.Count > 0)
            {
                for (int i = 0; i < MissingBooksList.Count; i++)
                {
                    if (StoredBooks.Count < 3)
                    {
                        StoredBooks.Add(MissingBooksList.ElementAt(i));
                    }
                }
            }
            base.DeSpawn();
        }
    }
}
