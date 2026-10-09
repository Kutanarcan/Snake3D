using System;
using System.Collections.Generic;
using Game.Authoring;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public sealed class LevelBrushTests
    {
        private static LevelDraft NewDraft()
        {
            return new LevelDraft(new GridSize(9, 9));
        }

        [Test]
        public void ThePaletteOffersEveryItemTypeThereIs()
        {
            List<string> names = new List<string>();

            foreach (LevelBrush brush in LevelBrush.All())
            {
                names.Add(brush.Name);
            }

            Assert.That(names, Is.EqualTo(new[] { "Erase", "Start", "Wall", "Tunnel" }),
                "A new CellGroundType must reach the palette without anyone editing a list.");
        }

        [Test]
        public void AnItemBrushPaintsItsOwnType()
        {
            LevelDraft draft = NewDraft();

            LevelBrush.Item(CellGroundType.Tunnel).Apply(draft, new GridPosition(2, 2));

            Assert.That(draft.Items.ItemAt(new GridPosition(2, 2)).Type, Is.EqualTo("Tunnel"));
        }

        [Test]
        public void TheEraseBrushClearsTheCell()
        {
            LevelDraft draft = NewDraft();
            draft.Items.Paint(new GridPosition(2, 2), CellGroundType.Wall);

            LevelBrush.Erase.Apply(draft, new GridPosition(2, 2));

            Assert.That(draft.Items.ItemAt(new GridPosition(2, 2)), Is.Null);
        }

        [Test]
        public void TheStartBrushMovesTheSnakeAndLeavesTheItemsAlone()
        {
            LevelDraft draft = NewDraft();
            draft.Items.Paint(new GridPosition(4, 1), CellGroundType.Wall);

            LevelBrush.Start.Apply(draft, new GridPosition(4, 1));

            Assert.That(draft.Start, Is.EqualTo(new GridPosition(4, 1)));
            Assert.That(draft.Items.ItemAt(new GridPosition(4, 1)), Is.Not.Null,
                "Moving the start is not a reason to rub an item out; Validate says so instead.");
            Assert.That(draft.Validate().IsSuccess, Is.False);
        }

        [Test]
        public void OpenIsTheEraseBrushRatherThanAnItem()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => LevelBrush.Item(CellGroundType.Open));
            Assert.That(LevelBrush.Erase.Erases, Is.True);
            Assert.That(LevelBrush.Start.Erases, Is.False, "Start shares Open but is not an eraser.");
        }
    }
}
