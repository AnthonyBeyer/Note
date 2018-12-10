using UnityEngine;
using System.Collections;
using System.Text;

public static class NoteCrypt 
{
	private static int[] encodingKeys = new int[5] { 7490, 458, 8804, 521, 1253 };
	public static string GetText(string text)
	{
		string result = "";
		int random = -1;
		char[] letters = text.ToCharArray();
		for (int i=0; i<letters.Length; i++) 
		{
			random ++;
			if (random > encodingKeys.Length-1) random = 0;
			int key = encodingKeys[random];
			if (char.ToString(letters[i]) == " ") result += " ";
			else result += (char)(letters[i] ^ key);
		}
		return result;
	}
}
