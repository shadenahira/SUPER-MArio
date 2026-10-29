using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Menu: Smario > Construir niveles 1-2, 1-3 y 1-4
// Genera las escenas automaticamente usando los prefabs y sprites del proyecto.
public static class SmarioLevelBuilder
{
    const string P = "Assets/Prefabs/";
    const string S = "Assets/Sprites/";
    const string X = "Assets/Sprites/Extra/";
    const string A = "Assets/Audio/";

    static Transform stage;

    [MenuItem("Smario/Construir niveles 1-2, 1-3 y 1-4")]
    public static void BuildAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        ConfigureExtraSprites();
        BuildUnderground();
        BuildWater();
        BuildCastle();
        LinkLevel11();
        UpdateBuildSettings();

        EditorSceneManager.OpenScene("Assets/Scenes/1-1.unity");
        EditorUtility.DisplayDialog("Smario", "¡Listo! Se crearon los niveles 1-2, 1-3 y 1-4.\n\nAbre 1-1 y pulsa Play.", "OK");
    }

    // ------------------------------------------------------------------ helpers

    static void ConfigureExtraSprites()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites/Extra" });
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 16;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.alphaIsTransparency = true;
            TextureImporterSettings st = new TextureImporterSettings();
            ti.ReadTextureSettings(st);
            st.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(st);
            ti.SaveAndReimport();
        }
    }

    static Sprite Spr(string path) { return AssetDatabase.LoadAssetAtPath<Sprite>(path); }
    static AudioClip Clip(string name) { return AssetDatabase.LoadAssetAtPath<AudioClip>(A + name + ".wav"); }
    static GameObject Pf(string name) { return AssetDatabase.LoadAssetAtPath<GameObject>(P + name + ".prefab"); }

    static GameObject Inst(string prefab, float x, float y)
    {
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(Pf(prefab), stage);
        go.transform.position = new Vector3(x, y, 0f);
        return go;
    }

    // Bloque solido (con colision) que ocupa celdas x..x+w-1, y..y+h-1
    static GameObject Solid(Sprite sprite, int x, int y, int w, int h, string name = "Block", Color? tint = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(stage);
        go.transform.position = new Vector3(x + (w - 1) * 0.5f, y + (h - 1) * 0.5f, 0f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(w, h);
        sr.sortingOrder = 5;
        if (tint.HasValue) sr.color = tint.Value;
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(w, h);
        return go;
    }

    // Decoracion sin colision
    static GameObject Deco(Sprite sprite, int x, int y, int w, int h, string name = "Deco", int order = 1)
    {
        GameObject go = Solid(sprite, x, y, w, h, name);
        Object.DestroyImmediate(go.GetComponent<BoxCollider2D>());
        go.GetComponent<SpriteRenderer>().sortingOrder = order;
        return go;
    }

    static GameObject Animated(string name, Sprite[] frames, float x, float y, float frame = 0.15f)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(stage);
        go.transform.position = new Vector3(x, y, 0f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = frames[0];
        sr.sortingOrder = 6;
        AnimatedSprite anim = go.AddComponent<AnimatedSprite>();
        anim.sprites = frames;
        anim.framerate = frame;
        return go;
    }

    static void Brick(int x, int y, Sprite look = null, GameObject item = null, int hits = -1)
    {
        GameObject b = Inst("Brick", x, y);
        if (look != null) b.GetComponent<SpriteRenderer>().sprite = look;
        BlockHit hit = b.GetComponent<BlockHit>();
        hit.item = item;
        hit.maxHits = hits;
    }

    static void Mystery(int x, int y, GameObject item = null)
    {
        GameObject b = Inst("MysteryBlock", x, y);
        if (item != null) b.GetComponent<BlockHit>().item = item;
    }

    static void Coin(int x, int y) { Inst("Coin", x, y); }

    static void PipeAt(int x, int height)
    {
        // tuberia vertical de 2 de ancho; la parte de arriba queda en y = 1 + height
        Inst("Pipe", x + 0.5f, 1 + height);
    }

    static Scene NewLevel(string name, Color background, string music, out GameObject mario, float marioX = 2f, float marioY = 2f)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camara
        GameObject cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        Camera c = cam.AddComponent<Camera>();
        c.orthographic = true;
        c.orthographicSize = 7f;
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = background;
        cam.AddComponent<AudioListener>();
        cam.transform.position = new Vector3(11.5f, 6.5f, -10f);

        // Game Manager
        new GameObject("Game Manager").AddComponent<GameManager>();

        // AudioManager (1º musica, 2º efectos)
        GameObject am = new GameObject("AudioManager");
        AudioSource musicSrc = am.AddComponent<AudioSource>();
        musicSrc.clip = Clip("Music/" + music);
        musicSrc.loop = true;
        musicSrc.playOnAwake = true;
        musicSrc.volume = 0.5f;
        AudioSource sfxSrc = am.AddComponent<AudioSource>();
        sfxSrc.playOnAwake = false;
        AudioManager manager = am.AddComponent<AudioManager>();
        manager.music = musicSrc;
        manager.sfx = sfxSrc;

        stage = new GameObject("Stage").transform;

        mario = Inst("Mario", marioX, marioY);

        // la camara sigue a Mario (se agrega despues de crear a Mario)
        cam.AddComponent<SideScrolling>();

        // Barrera de muerte para los hoyos
        GameObject death = new GameObject("Death Barrier");
        death.transform.SetParent(stage);
        death.transform.position = new Vector3(150f, -1f, 0f);
        BoxCollider2D dc = death.AddComponent<BoxCollider2D>();
        dc.isTrigger = true;
        dc.size = new Vector2(400f, 1f);
        death.AddComponent<DeathBarrier>();

        return scene;
    }

    static void ExitPipe(int x, int nextStage)
    {
        // Tuberia horizontal de salida (decorativa) + trigger que pasa al siguiente nivel
        GameObject pipe = Inst("PipeConnection", x, 2.5f);
        Pipe p = pipe.GetComponent<Pipe>();
        if (p != null) p.connection = null;

        GameObject exit = new GameObject("Level Exit");
        exit.transform.SetParent(stage);
        exit.transform.position = new Vector3(x - 0.75f, 2.5f, 0f);
        BoxCollider2D bc = exit.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(0.75f, 2f);
        LevelExit le = exit.AddComponent<LevelExit>();
        le.nextWorld = 1;
        le.nextStage = nextStage;
        le.enterSound = Clip("SFX/pipe");
        le.clearSound = Clip("SFX/stageclear");
    }

    static void Save(Scene scene, string name)
    {
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/" + name + ".unity");
    }

    // ------------------------------------------------------------------ 1-2 SUBTERRANEO (replica del World 1-2)

    static void BuildUnderground()
    {
        GameObject mario;
        Scene scene = NewLevel("1-2", Color.black, "underground", out mario, 3f, 2f);

        Sprite floor = Spr(S + "UndergroundBlock.png");
        Sprite brick = Spr(S + "UndergroundBrick.png");
        GameObject mushroom = Pf("MagicMushroom");
        GameObject star = Pf("Starman");
        GameObject oneUp = Pf("1upMushroom");
        GameObject coinItem = Pf("BlockCoin");

        // Suelo (2 filas) con hoyos
        Solid(floor, -2, 0, 82, 2, "Floor");
        Solid(floor, 83, 0, 20, 2, "Floor");
        Solid(floor, 106, 0, 22, 2, "Floor");
        Solid(floor, 134, 0, 40, 2, "Floor");

        // Pared izquierda y techo de ladrillos
        Solid(brick, -2, 2, 2, 11, "Wall");
        Solid(brick, 6, 12, 128, 1, "Ceiling");

        // Fila de bloques ? (el primero da champiñon)
        Mystery(10, 5, mushroom);
        for (int x = 11; x <= 14; x++) Mystery(x, 5);

        Inst("Goomba", 16, 2);
        Inst("Goomba", 17.5f, 2);

        // Columnas escalonadas
        int[] heights = { 1, 2, 3, 4, 4, 3, 3, 2 };
        int[] cols = { 24, 26, 28, 30, 32, 34, 38, 40 };
        for (int i = 0; i < cols.Length; i++) Solid(floor, cols[i], 2, 1, heights[i], "Step");
        Inst("Goomba", 36, 2);

        // Estructura de ladrillos con monedas y la estrella
        for (int x = 44; x <= 49; x++) if (x != 46) Brick(x, 6, brick);
        Brick(46, 6, brick, star, 1);
        for (int x = 44; x <= 49; x++) Coin(x, 7);
        Solid(brick, 44, 2, 1, 4, "Pillar");

        // "U" de ladrillos con monedas dentro
        Solid(brick, 52, 5, 1, 4, "U");
        Solid(brick, 58, 5, 1, 4, "U");
        for (int x = 53; x <= 57; x++) if (x != 55) Brick(x, 5, brick);
        Brick(55, 5, brick, coinItem, 10);
        for (int x = 53; x <= 57; x++) Coin(x, 6);
        Inst("Goomba", 54, 2);
        Inst("Goomba", 56, 2);

        // Bloque grande de ladrillos con monedas arriba
        Solid(brick, 63, 5, 6, 3, "BrickBlock");
        for (int x = 63; x <= 68; x++) Coin(x, 9);
        Inst("Koopa", 70, 2);

        // Hoyo 82, repisa de ladrillos y bloque 1-UP
        for (int x = 85; x <= 90; x++) Brick(x, 5, brick);
        Brick(88, 9, brick, oneUp, 1);
        Inst("Goomba", 92, 2);
        Inst("Goomba", 93.5f, 2);
        Solid(brick, 96, 2, 2, 3, "Wall");
        Solid(brick, 98, 2, 2, 5, "Wall");

        // Tuberias de distinta altura
        PipeAt(108, 3);
        PipeAt(114, 4);
        PipeAt(120, 2);
        Inst("Goomba", 111, 2);
        Inst("Koopa", 117, 2);

        // Gran hoyo con plataformas flotantes (en el original son elevadores)
        Solid(brick, 128, 5, 2, 1, "Platform");
        Solid(brick, 131, 7, 2, 1, "Platform");

        // Escalera final
        for (int i = 0; i < 6; i++) Solid(floor, 138 + i, 2, 1, i + 1, "Stairs");
        for (int x = 144; x <= 148; x += 2) Coin(x, 4);

        // Tuberia de salida -> 1-3 (camina hacia la derecha para entrar)
        ExitPipe(152, 3);
        Solid(brick, 157, 2, 2, 10, "End Wall");

        Save(scene, "1-2");
    }

    // ------------------------------------------------------------------ 1-3 ACUATICO (replica del World 2-2)

    static void BuildWater()
    {
        GameObject mario;
        Scene scene = NewLevel("1-3", new Color(0.13f, 0.22f, 0.93f), "underwater", out mario, 3f, 3f);

        PlayerMovement pm = mario.GetComponent<PlayerMovement>();
        pm.swimming = true;
        PrefabUtility.RecordPrefabInstancePropertyModifications(pm);

        Sprite floor = Spr(X + "SeaFloor.png");
        Sprite coral = Spr(X + "Coral.png");
        Sprite water = Spr(X + "WaterTop.png");
        Sprite[] fish = { Spr(X + "CheepCheep1.png"), Spr(X + "CheepCheep2.png") };
        Sprite[] squid = { Spr(X + "Blooper1.png"), Spr(X + "Blooper2.png") };

        // Superficie del agua (decorativa) y techo invisible
        Deco(water, -2, 12, 180, 1, "Water Surface");
        GameObject roof = new GameObject("Roof");
        roof.transform.SetParent(stage);
        roof.transform.position = new Vector3(88f, 13.5f, 0f);
        roof.AddComponent<BoxCollider2D>().size = new Vector2(190f, 1f);

        // Fondo del mar con hoyos
        Solid(floor, -2, 0, 68, 2, "Floor");
        Solid(floor, 70, 0, 30, 2, "Floor");
        Solid(floor, 104, 0, 22, 2, "Floor");
        Solid(floor, 130, 0, 46, 2, "Floor");

        // Corales y rocas
        Solid(coral, 12, 2, 1, 3, "Coral");
        Solid(floor, 18, 6, 4, 1, "Rock");
        Solid(coral, 26, 2, 1, 5, "Coral");
        Solid(floor, 32, 8, 6, 1, "Rock");
        Solid(coral, 41, 2, 1, 4, "Coral");
        Solid(floor, 46, 2, 3, 3, "Rock");
        Solid(floor, 54, 9, 5, 1, "Rock");
        Solid(coral, 62, 2, 1, 6, "Coral");
        Solid(floor, 74, 5, 6, 1, "Rock");
        Solid(coral, 84, 2, 1, 3, "Coral");
        Solid(coral, 90, 7, 1, 4, "Coral");
        Solid(floor, 96, 2, 4, 4, "Rock");
        Solid(floor, 108, 8, 6, 1, "Rock");
        Solid(coral, 116, 2, 1, 5, "Coral");
        Solid(floor, 122, 4, 4, 1, "Rock");
        Solid(coral, 134, 2, 1, 4, "Coral");
        Solid(floor, 140, 9, 6, 1, "Rock");

        // Monedas
        int[,] coins = { { 19, 7 }, { 20, 7 }, { 33, 9 }, { 34, 9 }, { 35, 9 }, { 55, 10 }, { 56, 10 }, { 57, 10 },
                         { 75, 6 }, { 76, 6 }, { 77, 6 }, { 109, 9 }, { 110, 9 }, { 111, 9 }, { 141, 10 }, { 142, 10 } };
        for (int i = 0; i < coins.GetLength(0); i++) Coin(coins[i, 0], coins[i, 1]);

        // Enemigos acuaticos
        float[,] cheeps = { { 22, 4 }, { 30, 9 }, { 44, 6 }, { 58, 3 }, { 72, 8 }, { 86, 5 }, { 100, 9 }, { 112, 4 }, { 126, 7 }, { 138, 5 } };
        for (int i = 0; i < cheeps.GetLength(0); i++) WaterFoe("Cheep Cheep", fish, cheeps[i, 0], cheeps[i, 1], false);
        float[] bloopers = { 36, 66, 94, 120, 146 };
        foreach (float bx in bloopers) WaterFoe("Blooper", squid, bx, 6f, true);

        // Salida: tuberia horizontal -> 1-4
        ExitPipe(152, 4);
        Solid(floor, 157, 2, 4, 10, "End Wall");

        Save(scene, "1-3");
    }

    static void WaterFoe(string name, Sprite[] frames, float x, float y, bool blooper)
    {
        GameObject go = Animated(name, frames, x, y, 0.2f);
        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = blooper ? new Vector2(0.8f, 1.2f) : new Vector2(0.8f, 0.7f);
        go.AddComponent<Hazard>();
        WaterEnemy we = go.AddComponent<WaterEnemy>();
        we.blooper = blooper;
        we.speed = 1.2f + (x % 3) * 0.3f;
    }

    // ------------------------------------------------------------------ 1-4 CASTILLO DE BOWSER (diseño propio)

    static void BuildCastle()
    {
        GameObject mario;
        Scene scene = NewLevel("1-4", Color.black, "castle", out mario, 2f, 6f);

        Sprite block = Spr(X + "CastleBlock.png");
        Sprite[] lava = { Spr(X + "Lava1.png"), Spr(X + "Lava2.png") };
        Sprite[] fire = { Spr(X + "Fire1.png"), Spr(X + "Fire2.png") };
        Sprite bridge = Spr(X + "Bridge.png");

        // Zona 1: entrada
        Solid(block, -2, 0, 18, 5, "Floor");
        Solid(block, -2, 10, 40, 3, "Ceiling");
        LavaPool(lava, 16, 4);
        MakePodoboo(fire, 17.5f, 1f, 6f, 0f);

        // Zona 2: barras de fuego
        Solid(block, 20, 0, 16, 4, "Floor");
        MakeFireBar(fire, 25, 6, 5, 100f);
        MakeFireBar(fire, 32, 4, 5, -120f);
        LavaPool(lava, 36, 5);
        MakePodoboo(fire, 38.5f, 1f, 7f, 1f);
        Solid(block, 37, 6, 3, 1, "Platform");

        // Zona 3: pasillo con techo bajo y monedas
        Solid(block, 41, 0, 22, 4, "Floor");
        Solid(block, 41, 9, 22, 4, "Low Ceiling");
        for (int x = 44; x <= 58; x += 2) Coin(x, 6);
        MakeFireBar(fire, 50, 4, 4, 140f);
        Inst("Goomba", 54, 4);
        Inst("Goomba", 56, 4);
        Mystery(47, 7, Pf("MagicMushroom"));

        // Zona 4: lago de lava con bloques flotantes
        LavaPool(lava, 63, 14);
        Solid(block, 65, 5, 2, 1, "Float");
        Solid(block, 69, 7, 2, 1, "Float");
        Solid(block, 73, 5, 2, 1, "Float");
        MakePodoboo(fire, 67.5f, 1f, 8f, 0.5f);
        MakePodoboo(fire, 71.5f, 1f, 8f, 1.7f);
        Solid(block, 77, 0, 8, 5, "Floor");
        Solid(block, 77, 11, 60, 2, "Ceiling");
        MakeFireBar(fire, 81, 8, 5, 90f);

        // Zona 5: puente de Bowser sobre lava
        Solid(block, 85, 0, 2, 5, "Bridge Post");
        LavaPool(lava, 87, 13);
        List<GameObject> pieces = new List<GameObject>();
        for (int x = 87; x < 100; x++) pieces.Add(Solid(bridge, x, 4, 1, 1, "Bridge"));

        GameObject bowserGo = new GameObject("Bowser");
        bowserGo.transform.SetParent(stage);
        bowserGo.transform.position = new Vector3(96.5f, 5.5f, 0f);
        SpriteRenderer bsr = bowserGo.AddComponent<SpriteRenderer>();
        bsr.sprite = Spr(X + "Bowser1.png");
        bsr.sortingOrder = 7;
        AnimatedSprite banim = bowserGo.AddComponent<AnimatedSprite>();
        banim.sprites = new[] { Spr(X + "Bowser1.png"), Spr(X + "Bowser2.png") };
        banim.framerate = 0.3f;
        BoxCollider2D bbc = bowserGo.AddComponent<BoxCollider2D>();
        bbc.isTrigger = true;
        bbc.size = new Vector2(1.6f, 1.8f);
        bowserGo.AddComponent<Hazard>();
        Bowser bowser = bowserGo.AddComponent<Bowser>();
        bowser.fireSprites = fire;
        bowser.fireSound = Clip("SFX/bowserfire");
        bowser.fallSound = Clip("SFX/bowserfall");

        // Plataforma final con el hacha
        Solid(block, 100, 0, 16, 5, "Floor");
        Solid(block, 115, 5, 1, 6, "Wall");
        GameObject axe = new GameObject("Axe");
        axe.transform.SetParent(stage);
        axe.transform.position = new Vector3(101f, 6f, 0f);
        SpriteRenderer asr = axe.AddComponent<SpriteRenderer>();
        asr.sprite = Spr(X + "Axe.png");
        asr.sortingOrder = 6;
        BoxCollider2D abc = axe.AddComponent<BoxCollider2D>();
        abc.isTrigger = true;
        abc.size = new Vector2(1f, 2f);
        BridgeAxe ba = axe.AddComponent<BridgeAxe>();
        ba.bridgePieces = pieces.ToArray();
        ba.bowser = bowser;
        ba.breakSound = Clip("SFX/breakblock");
        ba.winSound = Clip("SFX/stageclear");

        Save(scene, "1-4");
    }

    static void LavaPool(Sprite[] lava, int x, int w)
    {
        for (int i = 0; i < w; i++)
        {
            GameObject top = Animated("Lava", lava, x + i, 1f, 0.3f);
            GameObject below = Animated("Lava", lava, x + i, 0f, 0.3f);
            top.GetComponent<SpriteRenderer>().sortingOrder = 4;
            below.GetComponent<SpriteRenderer>().sortingOrder = 4;
            BoxCollider2D bc = top.AddComponent<BoxCollider2D>();
            bc.isTrigger = true;
            bc.size = new Vector2(1f, 0.6f);
            top.AddComponent<Hazard>().kill = true;
        }
    }

    static GameObject MakePodoboo(Sprite[] fire, float x, float y, float height, float offset)
    {
        GameObject go = Animated("Podoboo", fire, x, y, 0.1f);
        go.transform.localScale = new Vector3(2f, 2f, 1f);
        CircleCollider2D cc = go.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = 0.2f;
        go.AddComponent<Hazard>();
        Podoboo p = go.AddComponent<Podoboo>();
        p.height = height;
        p.offset = offset;
        return go;
    }

    static void MakeFireBar(Sprite[] fire, int x, int y, int length, float speed)
    {
        Solid(Spr(X + "CastleBlock.png"), x, y, 1, 1, "FireBar Base");
        GameObject pivot = new GameObject("FireBar");
        pivot.transform.SetParent(stage);
        pivot.transform.position = new Vector3(x, y, 0f);
        pivot.AddComponent<FireBar>().degreesPerSecond = speed;
        for (int i = 0; i < length; i++)
        {
            GameObject ball = new GameObject("Fire");
            ball.transform.SetParent(pivot.transform);
            ball.transform.localPosition = new Vector3(i * 0.5f, 0f, 0f);
            SpriteRenderer sr = ball.AddComponent<SpriteRenderer>();
            sr.sprite = fire[0];
            sr.sortingOrder = 8;
            AnimatedSprite anim = ball.AddComponent<AnimatedSprite>();
            anim.sprites = fire;
            anim.framerate = 0.1f;
            CircleCollider2D cc = ball.AddComponent<CircleCollider2D>();
            cc.isTrigger = true;
            cc.radius = 0.2f;
            ball.AddComponent<Hazard>();
        }
    }

    // ------------------------------------------------------------------ conexiones

    static void LinkLevel11()
    {
        Scene s = EditorSceneManager.OpenScene("Assets/Scenes/1-1.unity", OpenSceneMode.Single);
        FlagPole flag = Object.FindFirstObjectByType<FlagPole>();
        if (flag != null)
        {
            flag.nextWorld = 1;
            flag.nextStage = 2;
            EditorUtility.SetDirty(flag);
            PrefabUtility.RecordPrefabInstancePropertyModifications(flag);
        }
        EditorSceneManager.MarkSceneDirty(s);
        EditorSceneManager.SaveScene(s);
    }

    static void UpdateBuildSettings()
    {
        string[] order = { "1-1", "GameOver", "1-2", "1-3", "1-4" };
        List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene>();
        foreach (string n in order)
        {
            string path = "Assets/Scenes/" + n + ".unity";
            if (System.IO.File.Exists(path)) list.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = list.ToArray();
    }
}
