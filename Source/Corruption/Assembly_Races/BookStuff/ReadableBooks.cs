using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
namespace Corruption
{
    internal class ReadableBooks : ThingWithComps
    {
        public Pawn currentReader = null;
        private List<string> BookText = new List<string>();
        private int LifeSpam = 1500;
        public bool TexChange = false;
        private Graphic OpenBook;
        private List<string> DefaultText = new List<string>
		{
			"It was a dark and stormy night.",
			"Suddenly, a shot rang out!",
			"A door slammed.",
			"The maid screamed.",
			"Suddenly, a pirate ship appeared on the horizon!",
			"While millions of people were starving, the king lived in luxury.",
			"Meanwhile, on a small farm in Kansas, a boy was growing up.",
			"A light snow was falling....",
			"and the little girl with the tattered shawl had not sold a violet all day.",
			"At that very moment...",
			"a young intern at City Hospital was making an important discovery.",
			"The mysterious patient in Room 213 had finally awakened",
			"She moaned softly",
			"Could it be that she was the sister of the boy in Kansas...",
			"who loved the girl with the tattered shawl",
			"who was the daughter of the maid who had escaped from the pirates?",
			"The intern frowned."
		};
        public override Graphic Graphic
        {
            get
            {
                ReadFormXML();
                Graphic result;
                if (!TexChange && OpenBook != null)
                {
                    result = OpenBook;
                }
                else
                {
                    result = base.Graphic;
                }
                return result;
            }
        }
        public List<string> PrepareText()
        {
            ThingDef_Readables clutterThingDefs = (ThingDef_Readables)def;
            BookText = clutterThingDefs.BookText;
            List<string> result;
            if (BookText.Count > 0)
            {
                result = TextChooping(BookText);
            }
            else
            {
                result = TextChooping(DefaultText);
            }
            return result;
        }
        public override void Tick()
        {
            base.Tick();
            if (currentReader == null)
            {
                if (Find.Reservations.FirstReserverOf(this, factionInt) != null)
                {
                    currentReader = Find.Reservations.FirstReserverOf(this, factionInt);
                }
                if (LifeSpam <= 0)
                {
                    FeedBackPulse();
                }
                LifeSpam--;
            }
            else
            {
                if (currentReader.CurJob.def.defName != "ClutterSitAndRead")
                {
                    FeedBackPulse();
                }
            }
        }
        public void FeedBackPulse()
        {
            foreach (Thing current in Find.ListerThings.AllThings)
            {
                if (current.def.defName == "ClutterBookShelf")
                {
                    Bookshelf bookshelf = current as Bookshelf;
                    if (bookshelf.MissingBooksList.Contains(def))
                    {
                        if (bookshelf.StoredBooks.Count < 3)
                        {
                            bookshelf.MissingBooksList.Remove(def);
                            bookshelf.StoredBooks.Add(def);
                            if (Spawned)
                            {
                                Destroy();
                            }
                            break;
                        }
                    }
                }
            }
            if (Spawned)
            {
                Destroy();
            }
        }
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            ReadFormXML();
            if (OpenBook == null)
            {
                OpenBook = GraphicDatabase.Get<Graphic_Single>("Clutter/Books/CBook_Blue");
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.LookList<string>(ref BookText, "BookText", LookMode.Undefined, null);
            Scribe_References.LookReference<Pawn>(ref currentReader, "currentReader");
        }
        private List<string> TextChooping(List<string> textlist)
        {
            List<string> list = new List<string>();
            int num = Rand.RangeInclusive(0, textlist.Count - 7);
            int num2 = 0;
            for (int i = num; i < textlist.Count; i++)
            {
                list.Add(textlist[i]);
                num2++;
                if (num2 >= 7)
                {
                    break;
                }
            }
            return list;
        }
        private void ReadFormXML()
        {
            List<string> list = new List<string>();
            ThingDef_Readables clutterThingDefs = (ThingDef_Readables)def;
            if (clutterThingDefs.BookText.Count > 0)
            {
                list = clutterThingDefs.BookText;
            }
            if (!clutterThingDefs.CloseTexture.NullOrEmpty())
            {
                OpenBook = GraphicDatabase.Get<Graphic_Single>(clutterThingDefs.CloseTexture);
            }
        }
        public void Thoughts(Pawn reader)
        {
            int num = UnityEngine.Random.Range(1, 100);
            Thought thought = ThoughtMaker.MakeThought(ThoughtDef.Named("ReadBookGood"));
            Thought thought2 = ThoughtMaker.MakeThought(ThoughtDef.Named("ReadBookBad"));
            if (num < 10)
            {
                Log.Message("Bad Event:"+reader.Name.ToStringShort);
                if (!reader.needs.mood.thoughts.Thoughts.Contains(thought))
                {
                    foreach (Thought current in reader.needs.mood.thoughts.Thoughts)
                    {
                        if (current.def == thought.def)
                        {
                            reader.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDef.Named("ReadBookGood"));
                            break;
                        }
                    }
                    reader.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDef.Named("ReadBookBad"));
                }
                else
                {
                    reader.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDef.Named("ReadBookBad"));
                }
            }

            else
            {
                if (num > 70)
                {
                    Log.Message("Good Event:" + reader.Name.ToStringShort);
                    if (reader.needs.mood.thoughts.Thoughts.Contains(thought))
                    {
                        foreach (Thought current in reader.needs.mood.thoughts.Thoughts)
                        {
                            if (current.def == thought2.def)
                            {
                                reader.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDef.Named("ReadBookBad"));
                                break;
                            }
                        }
                        reader.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDef.Named("ReadBookGood"));
                    }
                    else
                    {
                        reader.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDef.Named("ReadBookGood"));
                    }
                }
            }
            
            FeedBackPulse();
        }
    }
}
