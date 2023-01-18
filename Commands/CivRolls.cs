using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
	public class CivRolls : BaseCommandModule
	{
		string[] civs =
			{"American", "Arabian", "Assyrian", "Austrian", "Aztec",
			"Babylonian", "Brazilian", "Byzantine", "Carthaginian", "Celtic",
			"CCP", "Danish", "VOC", "Egyptian", "Bri'ish",
			"Ethiopian", "French", "Germ*n", "Greek", "Hunnic",
			"Incan", "Ghandi", "Max's Homeland", "Iroquois", "Weebshit",
			"Korean", "Mayan", "Wollah", "Ottoman", "Persian",
			"Polish (ew)", "Polynesian", "Portuguese", "Roman", "vodka",
			"Shoshone", "Siamese", "Songhai", "Spanish", "Swedish twink",
			"HAHA VENETIE", "Zulu" };
		Dictionary<ulong, int[]> rolledCivs = new Dictionary<ulong, int[]>();
		private Random random = new Random();

		[Command("rollcivs")]
		public async Task RollCivs(CommandContext ctx)
		{
			await RollCivs(ctx, "5");
		}
		[Command("rollcivs")]
		public async Task RollCivs(CommandContext ctx, string amount)
		{
			int rolls = int.Parse(amount);
			int[] civIndexes = new int[rolls];

			//The code below generates a list of indexes for the civs array. It makes sure all indexes are unique.

			//When i=0 this for loop picks out of all civs in the civs array, which then can't be picked again. Thus leaving civs.Length-1 options, etc.
			for (int i = 0; i < rolls; i++)
				civIndexes[i] = random.Next(0, civs.Length - i);
			//If the index 6 has been chosen twice that means that out of the civs the civ with index 6 has been chosen
			//and out of the remaining civs (aka the civs minus civ 6) the civ with index 6 has been chosen.
			//This translates to civs with index 6 and 6+1(=7) being chosen.
			for (int i = rolls - 1; i >= 0; i--)
			{
				for (int j = i - 1; j >= 0; j--)
					if (civIndexes[i] >= civIndexes[j])
						civIndexes[i]++;
			}
			await OutputCivs(ctx, civIndexes);

		}
		[Command("reroll")]
		public async Task RerollCiv(CommandContext ctx, string index)
		{
			int[] civIndexes = rolledCivs[ctx.Member.Id];
			int indexInput = int.Parse(index);
			bool found = false;
			int civIndex = 0;
			while (!found)
			{
				found = true;
				civIndex = random.Next(0, civs.Length);
				for (int i = 0; i < civIndexes.Length; i++)
					if (civIndexes[i] == civIndex && i != indexInput) found = false;
			}
			civIndexes[indexInput] = civIndex;
			await OutputCivs(ctx, civIndexes);
		}
		public async Task OutputCivs(CommandContext ctx, int[] indexes)
		{
			QuickSort.SortQuick(indexes, 0, indexes.Length - 1);

			//Storing rolled civs for rerolling
			ulong username = ctx.Member.Id;
			if (rolledCivs.ContainsKey(username))
				rolledCivs[username] = indexes;
			else
				rolledCivs.Add(username, indexes);

			string message = "your civs are...";
			for (int i = 0; i < indexes.Length; i++)
				message += "\n-" + civs[indexes[i]];

			await ctx.RespondAsync(message);
		}
	}
	//Stolen from internet
	class QuickSort
	{
		static public int Partition(int[] numbers, int left, int right)
		{
			int pivot = numbers[left];

			while (true)
			{
				while (numbers[left] < pivot)
					left++;
				while (numbers[right] > pivot)
					right--;

				if (left < right)
				{
					int temp = numbers[right];
					numbers[right] = numbers[left];
					numbers[left] = temp;
				}
				else
				{
					return right;
				}
			}
		}

		static public void SortQuick(int[] arr, int left, int right)
		{
			// For Recusrion  
			if (left < right)
			{
				int pivot = Partition(arr, left, right);

				if (pivot > 1)
					SortQuick(arr, left, pivot - 1);

				if (pivot + 1 < right)
					SortQuick(arr, pivot + 1, right);
			}
		}
	}
}
