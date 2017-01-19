using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PushOverScene : kScene 
{
	public kSpriteAsset m_themeSprite;

	public Door			m_doorTemplate;
	public Domino		m_dominoTemplate;
	public Platform 	m_platformTemplate;
	public Ladder 		m_ladderTemplate;

	public Ant m_ant;

	public List<TextAsset> m_sourceLevels;

	public List<Level>	m_levels = new List<Level>();

	protected override void onInit(){
		base.onInit ();

		if (!Application.isPlaying)
			return;

		m_levels [0].StartLevel(m_ant);
	}

	public void LoadScene(){
		//for(int i = 0; i < m_sourceLevels.Count;i++ )
		//	ImportLevelFromFile( i + 1, m_sourceLevels[i]);
		ImportLevelFromFile( 6, m_sourceLevels[5]);
	}

	public void ImportLevelFromFile(int levelIndex,TextAsset sourceLevel)
	{
		Debug.Log("Import level" + sourceLevel.name);

		GameObject go = new GameObject ();
		Level level = go.AddComponent<Level> ();

		level.name = "Level" + levelIndex;
		level.transform.parent = transform.parent;
		level.transform.localPosition = new Vector3 ( -360, 310, -1);

		string bkgFrame = "BKG_LVL" + levelIndex;
		level.SetupLevel (m_themeSprite, bkgFrame);

		int layer = 0;

		string[] textLines =  sourceLevel.text.Split('\n');
		int crtLine = 0;
		while(crtLine < textLines.Length)
		{
			while(crtLine < textLines.Length && !textLines[crtLine].Contains("Level") && !textLines[crtLine].Contains("Background")){
				crtLine++;
			}

			if (crtLine >= textLines.Length)
				break;

			if(textLines[crtLine].Contains("Level"))
			{
				while(textLines[crtLine][0] != '|' && crtLine < textLines.Length){
					crtLine++;
				}
				string[] lines = new string[13];
				lines[0] = textLines[crtLine].Substring(1,textLines[crtLine].Length - 1) + "                    ";
				for(int i = 1; i < 13; i++){
					crtLine ++;
					textLines[crtLine] += "                           ";
					lines[i] = textLines[crtLine].Substring(2,textLines[crtLine].Length - 2); 
				}
				ReadLevelObjects( level, lines);
			}else
			{
				while((textLines[crtLine].Length == 0 || textLines[crtLine][0] != '|') && crtLine < textLines.Length){
					crtLine++;
				}
				//create a frame
				//Module m = (Module)img.modules.get(firstBackgroundModule);
				//Point pos = new Point(-m.getBounds().width/2,-m.getBounds().height); 

				//layers Frame newFrame = new Frame(m_sprite,"BKG_LVL"+level+"_LAYER_"+layer);
				layer++;
				//Frame.FrameComponent fComp = newFrame.new FrameComponent(m_sprite,m,
				//							 pos,0,1.0f,1.0f,0xFF,0);
				//newFrame.getComponents().add(fComp);
				//layers m_sprite.getFrames().add(newFrame);
				int x = 0, y = 0;

				while(textLines[crtLine].Length > 0 && textLines[crtLine][0] == '|' && crtLine < textLines.Length){
					int start = -1;
					int i = 0;
					while(i < textLines[crtLine].Length){
						while(i < textLines[crtLine].Length && (textLines[crtLine][i] == '|' || textLines[crtLine][i] == ' '))
							i++;

						if(i < textLines[crtLine].Length){
							start = i;
							while(i < textLines[crtLine].Length && textLines[crtLine][i] != ' ')
								i++;

							if(i < textLines[crtLine].Length)
							{
								string moduleIndexStr = textLines[crtLine].Substring(start, i - start);
								int moduleIndex = Int32.Parse(moduleIndexStr);

								//create a frame
								//Module m = (Module)DataController.m_sprite.getSelectedImage().modules.get(noForegroundTiles[theme] + moduleIndex);
								Vector2 pos = new Vector2(x,y); 
								/* layers Frame.FrameComponent fComp = newFrame.new FrameComponent(m_sprite,m,
															 pos,0,1.0f,1.0f,0xFF,0);
								newFrame.getComponents().add(fComp);
								*/
								//fullframe
								//Frame.FrameComponent fComp1 = bkgFrame.new FrameComponent(DataController.m_sprite,m,
								//	pos,0,1.0f,1.0f,0xFF,0);
								//bkgFrame.getComponents().add(fComp1);

								x += PushOverLevelConstants.tileW;
							}
						}
					}
					crtLine++;
					y += PushOverLevelConstants.tileH;
					x = 0;
				}
			}
		}
		level.SetupPathfinding (new Vector2(20,20));
		level.gameObject.SetActive (false);

		m_levels.Add (level);
	}

	private void ReadLevelObjects(Level level, string[] lines){
		int dominoXOFf = 20;
		int dominoYOFf = -12;

		string domino = null;

		for(int y = 0; y < 13; y++ )
		{
			for(int x = 0; x < 20; x++)
			{
				if(x >= lines[y].Length)
					break;

				domino = null;
				switch (lines[y][x]) {

				case '1':
					level.AddObject ( m_doorTemplate, "Door_0", x * PushOverLevelConstants.tileW, y * PushOverLevelConstants.tileH);
					break;
				case '2':
					level.AddObject ( m_doorTemplate, "Door_1", x * PushOverLevelConstants.tileW, y * PushOverLevelConstants.tileH);
					break;
				case '$':
					level.AddObject ( m_dominoTemplate, "DominoStandard", (x + 1) * PushOverLevelConstants.tileW - dominoXOFf,y * PushOverLevelConstants.tileH - dominoYOFf);
					break;
				case '\\':
					level.AddObject ( m_platformTemplate, "Platform_Step_1", (x - 1)* PushOverLevelConstants.tileW,(y - 1) * PushOverLevelConstants.tileH);
					level.AddObject ( m_platformTemplate, "Platform_Step_2", x * PushOverLevelConstants.tileW,(y - 1) * PushOverLevelConstants.tileH);
					level.AddObject ( m_platformTemplate, "Platform_Step_3", (x - 1) * PushOverLevelConstants.tileW,y * PushOverLevelConstants.tileH);
					level.AddObject ( m_platformTemplate, "Platform_Step_4", x * PushOverLevelConstants.tileW,y * PushOverLevelConstants.tileH);
					break;

				case '/':
					level.AddObject ( m_platformTemplate, "Platform_Step_5", x * PushOverLevelConstants.tileW,(y - 1) * PushOverLevelConstants.tileH);
					level.AddObject ( m_platformTemplate, "Platform_Step_6", (x + 1) * PushOverLevelConstants.tileW,(y - 1) * PushOverLevelConstants.tileH);
					level.AddObject ( m_platformTemplate, "Platform_Step_7", x * PushOverLevelConstants.tileW,y * PushOverLevelConstants.tileH);
					break;
				case ' ':
				case '^':
					if (x > 0 && lines[y][x - 1] == '/')
					{
						level.AddObject ( m_platformTemplate, "Platform_Step_8", x * PushOverLevelConstants.tileW,y * PushOverLevelConstants.tileH);
					}
					break;
				case 'H':
				case 'V':
					level.AddObject (m_ladderTemplate, "Ladder", x * PushOverLevelConstants.tileW,y * PushOverLevelConstants.tileH);
					break;

				case '.':
					level.AddObject (m_platformTemplate, "Platform_Strip", x * PushOverLevelConstants.tileW,y * PushOverLevelConstants.tileH);
					break;

				case 'I':
					domino = "DominoStandard";
					break;
				case  '#':
					domino = "DominoStopper";
					break;
				case 'Y':
					domino = "DominoSplitter";
					break;
				case '*':
					domino = "DominoExploder";
					break;
				case '?':
					domino = "DominoDelay";
					break;
				case 'O':
					domino = "DominoTumbler";
					break;
				case '=':
					domino = "DominoBridger";
					break;
				case ':':
					domino = "DominoVanish";
					break;
				case '!':
					domino = "DominoTrigger";
					break;
				case 'A':
					domino = "DominoAscender";
					break;
				case 'X':
				case 'x':
					domino = "DominoConnectedA";

					break;
				}

				if(PushOverLevelConstants.IsDominoChar(lines[y][x]))//domino != null)
				{

					bool ladderAbove   = y > 0 && lines[y - 1][x] == 'H';
					bool ladderBelow   = y + 1 < 13 && (lines[y + 1][x] == 'H' || lines[y + 1][x] == 'V' || lines[y + 1][x] == '^');

					bool platformLeft  = x <= 0		|| PushOverLevelConstants.IsDominoChar(lines[y][x - 1]) || lines[y][x - 1] == '\\';
					bool platformRight = x + 1 >= 20|| PushOverLevelConstants.IsDominoChar(lines[y][x + 1]) || lines[y][x + 1] == '/';

					if (ladderBelow) {
						//platform = "Platform_Ladder_Down";
						level.AddObject (m_platformTemplate, "Platform_Middle", x * PushOverLevelConstants.tileW, y * PushOverLevelConstants.tileH);
						level.AddObject (m_ladderTemplate, "Ladder", x * PushOverLevelConstants.tileW, y * PushOverLevelConstants.tileH);
					} else if (ladderAbove) {
						//platform = "Platform_Ladder_Up";
						level.AddObject (m_platformTemplate, "Platform_Middle", x * PushOverLevelConstants.tileW,y * PushOverLevelConstants.tileH);
						level.AddObject (m_ladderTemplate, "Ladder", x * PushOverLevelConstants.tileW,y * PushOverLevelConstants.tileH);
					}else if (!platformLeft && !platformRight)
						level.AddObject (m_platformTemplate, "Platform_Strip", x * PushOverLevelConstants.tileW,y * PushOverLevelConstants.tileH);
					else if (!platformLeft)
						level.AddObject (m_platformTemplate, "Platform_Start", x * PushOverLevelConstants.tileW,y * PushOverLevelConstants.tileH);
					else if (!platformRight)
						level.AddObject (m_platformTemplate, "Platform_End", x * PushOverLevelConstants.tileW,y * PushOverLevelConstants.tileH);
					else
						level.AddObject (m_platformTemplate, "Platform_Middle", x * PushOverLevelConstants.tileW,y * PushOverLevelConstants.tileH);
				
					if(domino != null)
					{
						level.AddObject (m_dominoTemplate, domino, (x + 1) * PushOverLevelConstants.tileW - dominoXOFf, y * PushOverLevelConstants.tileH - dominoYOFf);
					}
				}
			}
		}
	}
}
