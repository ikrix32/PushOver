using UnityEngine;
using System.Collections;

public class PushOverLevelConstants {

	public static int tileW   = 40;
	public static int tileH   = 48;
	static int[] noForegroundTiles = new int[]{21,21,21,21,21,21,21,21,21,21};
	static int[] noBackgroundTiles = new int[]{124,286,173,170,187,257,284,308,235,341};

	static string[] foregroundTileNames = {
		"Platform_Start",
		"Platform_Middle",
		"Platform_End",
		"Platform_Ladder_Down",
		"Ladder",
		"Platform_Ladder_Up",
		"Platform_Step_1",
		"Platform_Step_2",
		"Platform_Step_3",
		"Platform_Step_4",
		"Platform_Step_5",
		"Platform_Step_6",
		"Platform_Step_7",
		"Platform_Step_8",
		"Ladder_Middle",
		"Platform_Strip",
		"Ladder2",
		"Door_0",
		"Door_1",
		"Door_2",
		"Door_3",
	};

	public static bool IsDominoChar(char c){
		switch(c)
		{
		case '_':
		case 'I':
		case  '#':
		case 'Y':
		case '*':
		case '?':
		case 'O':
		case '=':
		case ':':
		case '!':
		case 'A':
		case 'X':
		case 'x':
			return true;
		}
		return false;
	}


	static int[][] foregroundTilePos = new int[][]{ 
		//aztec
		new int[]{ 1, 0,2, 0,3, 0,4, 0,5, 0,6, 0,7, 0,8, 0,9, 0,10,0,11, 0,12, 0,13, 0,14, 0,17, 0,18, 0,5, 0,20, 0,21, 0,22, 0,23, 0,},
		//castle
		new int[]{  0, 0, 0, 1, 0, 2, 0, 3, 0, 4, 0, 5, 0, 6,0, 7,0, 8,0, 9,0, 10,0, 11,0, 12,0, 13,0, 16,0, 17, 0, 18,0, 19,0, 20,0, 21,0, 22,},
		//cavern
		new int[]{  0, 0, 0, 1, 0, 2, 0, 3, 0, 4, 0, 5, 0, 6, 0, 7, 0, 8, 0, 9, 0, 10, 0, 11,  0, 12, 0, 13, 0, 16, 0, 17, 0, 18, 0, 19, 0, 20, 0, 21, 0, 22, },
		//dungeon
		new int[]{  1, 0,2, 0,3, 0,4, 0, 5, 0,6, 0,7, 0,8, 0,9, 0,10, 0,11, 0,12, 0,13, 0,14, 0,17, 0,18, 0,5, 0,20, 0,21, 0,22, 0,23, 0,},
		//toxcity
		new int[]{  0, 0, 1, 0, 2, 0, 3, 0, 4, 0, 5, 0, 6, 0, 7, 0, 8, 0, 9, 0, 10, 0, 11, 0, 12, 0, 13, 0, 1, 1, 2, 1, 3, 1, 4, 1, 5, 1, 6, 1, 7, 1, },
		//space
		new int[]{  1, 0, 2, 0, 3, 0,  4, 0, 5, 0, 6, 0, 7, 0,  8, 0,  9, 0, 10, 0, 11, 0, 12, 0, 13, 0, 14, 0, 17, 0, 18, 0, 5, 0, 20, 0,  21, 0, 22, 0, 23, 0, },
		//electro
		new int[]{ 1, 0, 2, 0, 3, 0, 4, 0, 5, 0, 6, 0, 7, 0, 8, 0, 9, 0, 10, 0, 11, 0, 12, 0, 13, 0, 14, 0, 17, 0, 18, 0,  5, 0, 20, 0, 21, 0, 22, 0, 23, 0, },
		//greek
		new int[]{  0, 0,  0, 1, 0, 2, 0, 3,  0, 4,  0, 5,  0, 6,  0, 7, 0, 8, 0, 9, 0, 10, 0, 11, 0, 12, 0, 13, 0, 16, 0, 17, 0, 18, 0, 19, 0, 20, 0, 21, 0, 22, },
		//mechanics
		new int[]{ 1, 2,2, 2,3, 2,6, 2, 6, 3,6, 4,7, 4,8, 4,7, 5,8, 5,11, 3,12, 3,13, 3,14, 3,9, 2,1, 4, 9, 1,18, 0,19, 0,20, 0,21, 0,},
		//japaness
		new int[]{ 0, 0,0, 1,0, 2,0, 3,0, 4,0, 5,0, 6,0, 7,0, 8,0, 9,0, 10,0, 11,0, 12,0, 13,0, 16,0, 17,0, 18,0, 19,0, 20,0, 21,0, 22},
	};
}
