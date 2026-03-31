using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using System;
using System.Linq;
using System.IO;

namespace CoffeeShop
{
    public class UIManager
    {
        private enum WorkstationPhase
        {
            SelectingIngredients,
            AnimatingBrewActions,
            AwaitingServeOrTrashChoice
        }

        private int _screenWidth;
        private int _screenHeight;

        public bool IsMenuOpen { get; set; }
        private GameView _currentGameView;
        private GameView _previousView;
        private bool _isTransitioning;
        private Font _mainFont;

        // Common Textures
        private Texture2D _menuBackgroundTexture;
        private Texture2D _settingsIconTexture;
        private Texture2D _customerScreenBgTexture;
        private Texture2D _settingsScreenBgTexture;
        private Texture2D _levelIntroScreenBgTexture;
        private Texture2D _pauseButtonTexture;
        private Texture2D _dialogueBgTexture;
        private Texture2D _nightCoffeeBgTexture;
        private Rectangle _gameCompleteRestartButtonRect;
        private Rectangle _gameCompleteMenuButtonRect;

        // Workstation Screen Specific Textures
        private Texture2D _workstationSpritesheet1;
        private Texture2D _workstationSpritesheet2;
        private Texture2D _blackBoxTexture;
        private Texture2D _whiteBoxTexture;
        private Texture2D _workstationButtonSpritesheet;
        private Texture2D _serveButtonTexture;
        private Texture2D _trashButtonTexture;
        private Rectangle _customerScreenTrashCanRect;

        // Workstation Screen Source Rectangles from Spritesheets
        private Rectangle _wsBackgroundSourceRect;
        private Rectangle _wsShelfSourceRect;
        private Rectangle _wsTemplateSourceRect;
        private Rectangle _wsInputBoxSourceRect;
        private Dictionary<IngredientType, Rectangle> _wsIngredientNormalIconSourceRects;
        private Dictionary<IngredientType, Rectangle> _wsIngredientHoverIconSourceRects;
        private Dictionary<IngredientType, Rectangle> _wsIngredientActionAnimSourceRects;
        private Rectangle _wsPropertyMeterAreaSourceRect;
        private Rectangle _wsResetButtonSourceRect;
        private Rectangle _wsBrewButtonSourceRect;
        private Rectangle _wsCupSourceRect;
        private Rectangle _wsHandSourceRect;
        private Rectangle _wsServingBgSourceRect;
        private Rectangle _wsTrashIconSourceRect;
        private Rectangle _wsServeIconSourceRect;


        private Texture2D _waterJugTexture;
        private Rectangle _waterMeterBackgroundRect;
        private Rectangle _useWaterJugButtonRect;
        private Rectangle _refillWaterButtonRect;
        private InteractionContext _currentWorkstationMachineContext;
        private const float REFILL_BUTTON_WIDTH = 180;
        private const float REFILL_BUTTON_HEIGHT = 35;
        private const float REFILL_BUTTON_OFFSET_X = 10;
        private const float REFILL_BUTTON_Y_POSITION = 400;

        private const int WATER_METER_WIDTH = 25;
        private const int WATER_METER_HEIGHT = 120;
        private const int WATER_METER_SCREEN_OFFSET_X = 40;
        private const int WATER_METER_SCREEN_OFFSET_Y = 150;
        private const int WATER_JUG_ICON_SIZE = 50;
        private const int WATER_JUG_UI_OFFSET_Y = 20;
        private Dictionary<string, Rectangle> _wsDrinkOutputSourceRects;

        private WorkstationPhase _currentWorkstationPhase;
        private List<IngredientType> _displayableIngredientTypesOnShelf;
        private string _workstationMessage;
        private bool _workstationMessageIsError;
        private float _workstationMessageTimer;
        private const float WORKSTATION_MESSAGE_DURATION = 2.0f;
        private const int LOW_STOCK_THRESHOLD = 10;
        private Rectangle _shopAccessButtonDest;
        private bool _isLowStockWarningActive = false;
        private bool _workstationBrewSuccess;
        private Drink? _workstationPreparedDrink;

        private float _brewActionAnimationTimer;
        private int _currentBrewActionIndex;
        private const float BREW_ACTION_ANIM_DURATION_PER_STEP = 1.0f;

        private float _menuBgFrameWidth, _menuBgFrameHeight, _menuBgFrameTimer, _menuBgFrameDuration;
        private int _menuBgTotalFrames, _menuBgCurrentFrame;
        private bool _menuBgSpriteSheetLoadedSuccessfully = false;

        private Rectangle _playButtonDest, _exitButtonDest, _settingsIconDestRect;
        private Rectangle _settingsScreenBackButtonDest, _pauseButtonScreenRect;
        private Rectangle _resumeButtonDest, _pauseSettingsButtonDest, _pauseMainMenuButtonDest, _pauseExitGameButtonDest;

        private List<Tuple<Ingredient, Rectangle, Rectangle>> _shopIngredientLayout;
        private Vector2 _shopScrollOffset;

        private Texture2D _shopBackgroundTexture;
        private Rectangle _shopCloseButtonDest;
        private const int SHOP_ITEM_ENTRY_HEIGHT = 40;
        private const int SHOP_BUTTON_WIDTH = 70;
        private const int SHOP_BUTTON_HEIGHT = 25;
        private const int SHOP_LIST_START_Y = 150;
        private const int SHOP_LIST_ITEM_PADDING = 5;
        private const float SHOP_SCROLL_SPEED_MULTIPLIER = 10.0f;

        private Vector2 _customerQueueSlot1Pos = new Vector2(110, 250);
        private Vector2 _customerQueueSlot2Pos = new Vector2(420, 250);
        private Vector2 _customerQueueSlot3Pos = new Vector2(750, 250);
        private float _customerDrawScale = 1.0f;
        private List<Tuple<Customer, Rectangle>> _customerClickAreas = new List<Tuple<Customer, Rectangle>>();

        private float _dialoguePortraitFrameTimer, _dialoguePortraitFrameDuration = 0.25f;

        private Rectangle _wsShelfDestRect;
        private Rectangle _wsTemplateDestRect;
        private List<Rectangle> _wsIngredientDisplayDestRectsOnShelf;
        private List<Rectangle> _wsInputBoxVisualDestRects;
        private List<Rectangle> _wsSelectedIngredientTextDestRects;
        private Rectangle _wsPropertyMeterAreaDestRect;
        private Dictionary<string, List<Rectangle>> _wsPropertyMeterBoxesDestRects;
        private Rectangle _wsResetButtonDestRect;
        private Rectangle _wsBrewButtonDestRect;
        private List<Rectangle> _wsBrewActionAnimDestRects;
        private Rectangle _wsServingAreaDestRect;
        private Rectangle _wsCupDestRect, _wsHandDestRect, _wsPreparedDrinkDestRect;
        private Rectangle _wsServeButtonDestRect, _wsTrashButtonDestRect;
        private Texture2D _patienceMeterTexture;
        private Rectangle[] _patienceMeterFrames;

        private Texture2D _sanityMeterTexture;
        private Rectangle _sanityMeterBorderSourceRect;
        private Rectangle _sanityMeterFillSourceBaseRect;

        private const int MAX_PATIENCE_FRAMES = 6;
        private Vector2 _patienceMeterScreenPos;
        private Vector2 _sanityMeterHudPos;
        private Vector2 _sanityMeterScreenPos;
        private float _queuePatienceMeterScale = 0.65f;
        private Vector2 _sanityMeterCustomerScreenPos;

        private const int MAX_SELECTED_INGREDIENTS = 3;
        private const int NUM_METER_SEGMENTS = 8;
        private const int NUM_SHELF_INGREDIENTS = 10;

        private Texture2D _heatwaveEffectTexture;
        private Rectangle[]? _heatwaveFrames;
        private int _currentHeatwaveFrame;
        private float _heatwaveFrameTimer;
        private float _heatwaveFrameDuration;


        private const int HEATWAVE_TOTAL_FRAMES = 10;
        private const int HEATWAVE_COLS = 2;
        private const int HEATWAVE_ROWS = 5;
        private float _heatwaveZoomFactor = 1.0f;
        private Texture2D _hiddenToyolIconTexture;
        private Texture2D _ghostScreenEffectTexture;
        private Rectangle[]? _ghostScreenEffectFrames;
        private int _currentGhostScreenEffectFrame;
        private float _ghostScreenEffectFrameTimer;
        private float _ghostScreenEffectFrameDuration;
        private float _ghostEffectZoomFactor = 1.0f;
        private const int GHOST_EFFECT_TOTAL_FRAMES = 10;
        private const int GHOST_EFFECT_COLS = 2;
        private const int GHOST_EFFECT_ROWS = 5;
        private Texture2D _customCursorTexture;
        private Vector2 _osMousePosition;
        public Vector2 GameCursorPosition { get; private set; }

        private float _masterVolume;
        private Rectangle _volumeSliderBarRect;
        private Rectangle _volumeSliderKnobRect;
        private bool _isDraggingVolumeKnob;

        private const float VOLUME_SLIDER_WIDTH = 200f;
        private const float VOLUME_SLIDER_HEIGHT = 10f;
        private const float VOLUME_SLIDER_POS_Y = 250f;
        private const float VOLUME_KNOB_RADIUS = 15f;

        private float _lastClickProcessedTime = 0f;
        private const float CLICK_COOLDOWN_DURATION = 0.2f;

        private const float NORMAL_CURSOR_LERP_SPEED = 0.75f;
        private const float GHOST_CURSOR_LERP_BASE_SPEED = 0.05f;

        private GameManager? _gameManagerInstance;

        public UIManager(int screenWidth, int screenHeight)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _shopAccessButtonDest = new Rectangle(_screenWidth - 120, _screenHeight - 60, 100, 40);
            _isTransitioning = false;
            _currentGameView = GameView.MainMenuScreen;
            _currentWorkstationPhase = WorkstationPhase.SelectingIngredients;
            _workstationMessage = string.Empty;

            _wsIngredientNormalIconSourceRects = new Dictionary<IngredientType, Rectangle>();
            _wsIngredientHoverIconSourceRects = new Dictionary<IngredientType, Rectangle>();
            _wsIngredientActionAnimSourceRects = new Dictionary<IngredientType, Rectangle>();
            _wsDrinkOutputSourceRects = new Dictionary<string, Rectangle>();
            _wsIngredientDisplayDestRectsOnShelf = new List<Rectangle>();
            _wsInputBoxVisualDestRects = new List<Rectangle>();
            _wsSelectedIngredientTextDestRects = new List<Rectangle>();
            _wsPropertyMeterBoxesDestRects = new Dictionary<string, List<Rectangle>>();
            _wsBrewActionAnimDestRects = new List<Rectangle>();
            _displayableIngredientTypesOnShelf = new List<IngredientType> {
                IngredientType.CoffeeBean, IngredientType.Milk, IngredientType.GreenTea, IngredientType.Tea, IngredientType.Chocolate,
                IngredientType.Ginger, IngredientType.Mint, IngredientType.Lemon, IngredientType.Honey, IngredientType.Cinnamon
            };

            _shopCloseButtonDest = new Rectangle(_screenWidth - 170, _screenHeight - 70, 150, 50);
            _shopIngredientLayout = new List<Tuple<Ingredient, Rectangle, Rectangle>>();
            _shopScrollOffset = Vector2.Zero;

            _shopCloseButtonDest = new Rectangle(
                _screenWidth - 220, _screenHeight - 70,
                200, 50
            );

            float buttonWidth = 280; float buttonHeight = 70; float commonButtonSpacing = 25;
            float firstButtonY = _screenHeight / 2f - (buttonHeight * 2 + commonButtonSpacing) / 2f + 50f;
            _playButtonDest = new Rectangle((_screenWidth - buttonWidth) / 2f, firstButtonY, buttonWidth, buttonHeight);
            _exitButtonDest = new Rectangle((_screenWidth - buttonWidth) / 2f, firstButtonY + buttonHeight + commonButtonSpacing, buttonWidth, buttonHeight);
            float genIconSize = 50;
            _settingsIconDestRect = new Rectangle(_screenWidth - genIconSize - 15, 15, genIconSize, genIconSize);
            _settingsScreenBackButtonDest = new Rectangle((_screenWidth - 200f) / 2f, _screenHeight - 120, 200, 60);
            float pauseButtonSize = 55;
            _pauseButtonScreenRect = new Rectangle(_screenWidth - pauseButtonSize - 10, 10, pauseButtonSize, pauseButtonSize);
            float pauseMenuButtonWidth = 280; float pauseMenuButtonHeight = 60; float pauseMenuButtonSpacing = 20;
            float pauseMenuFirstButtonY = _screenHeight / 2f - (pauseMenuButtonHeight * 4 + pauseMenuButtonSpacing * 3) / 2f;
            _resumeButtonDest = new Rectangle((_screenWidth - pauseMenuButtonWidth) / 2f, pauseMenuFirstButtonY, pauseMenuButtonWidth, pauseMenuButtonHeight);
            _pauseSettingsButtonDest = new Rectangle((_screenWidth - pauseMenuButtonWidth) / 2f, pauseMenuFirstButtonY + (pauseMenuButtonHeight + pauseMenuButtonSpacing), pauseMenuButtonWidth, pauseMenuButtonHeight);
            _pauseMainMenuButtonDest = new Rectangle((_screenWidth - pauseMenuButtonWidth) / 2f, pauseMenuFirstButtonY + (pauseMenuButtonHeight + pauseMenuButtonSpacing) * 2, pauseMenuButtonWidth, pauseMenuButtonHeight);
            _pauseExitGameButtonDest = new Rectangle((_screenWidth - pauseMenuButtonWidth) / 2f, pauseMenuFirstButtonY + (pauseMenuButtonHeight + pauseMenuButtonSpacing) * 3, pauseMenuButtonWidth, pauseMenuButtonHeight);

            float CompleteButtonWidth = 220;
            float CompleteButtonHeight = 50;
            float buttonSpacing = 20;
            float totalButtonWidth = (buttonWidth * 2) + buttonSpacing;

            _gameCompleteRestartButtonRect = new Rectangle(
                (screenWidth - totalButtonWidth) / 2, 
                screenHeight / 2 + 80, 
                CompleteButtonWidth, 
                CompleteButtonHeight
            );
            _gameCompleteMenuButtonRect = new Rectangle(
                _gameCompleteRestartButtonRect.X + buttonWidth + buttonSpacing,
                _gameCompleteRestartButtonRect.Y,
                CompleteButtonWidth,
                CompleteButtonHeight
            );

            float shelfSpriteOriginalWidth = 352f;
            float shelfSpriteOriginalHeight = 97f;
            float shelfDesiredDisplayWidth = _screenWidth * 0.65f;
            float shelfDisplayScale = shelfDesiredDisplayWidth / shelfSpriteOriginalWidth;
            float shelfDestActualWidth = shelfSpriteOriginalWidth * shelfDisplayScale;
            float shelfDestActualHeight = shelfSpriteOriginalHeight * shelfDisplayScale;
            _wsShelfDestRect = new Rectangle(
                (_screenWidth - shelfDestActualWidth) / 2f, 120f,
                shelfDestActualWidth, shelfDestActualHeight);

            int ingDestSizeOnShelf = (int)(shelfDestActualHeight * 0.48f);
            int ingDestSpacingXOnShelf = (int)(ingDestSizeOnShelf * 0.2f);
            int shelfItemsPerRow = 5;

            float totalShelfContentWidth = shelfItemsPerRow * ingDestSizeOnShelf + (shelfItemsPerRow - 1) * ingDestSpacingXOnShelf;
            float shelfContentStartX = _wsShelfDestRect.X + (shelfDestActualWidth - totalShelfContentWidth) / 45.0f;
            float iconRowOffsetY = shelfDestActualHeight * -0.45f;
            float shelfRow1Y = _wsShelfDestRect.Y + iconRowOffsetY;
            float rowSpacingFactor = 0.50f;
            float shelfRow2Y = shelfRow1Y + ingDestSizeOnShelf + (ingDestSizeOnShelf * rowSpacingFactor * 1.0f);

            _wsIngredientDisplayDestRectsOnShelf.Clear();
            for (int i = 0; i < NUM_SHELF_INGREDIENTS; i++)
            {
                int row = i / shelfItemsPerRow;
                int col = i % shelfItemsPerRow;
                _wsIngredientDisplayDestRectsOnShelf.Add(new Rectangle(
                    shelfContentStartX + col * (ingDestSizeOnShelf + ingDestSpacingXOnShelf),
                    row == 0 ? shelfRow1Y : shelfRow2Y,
                    ingDestSizeOnShelf, ingDestSizeOnShelf));
            }

            float templateVisualSourceWidth = 396f;
            float templateVisualSourceHeight = 178f;
            float templateDesiredDisplayHeight = _screenHeight * 0.50f;
            float templateDisplayScale = templateDesiredDisplayHeight / templateVisualSourceHeight;
            float templateDestActualWidth = templateVisualSourceWidth * templateDisplayScale;
            float templateDestActualHeight = templateVisualSourceHeight * templateDisplayScale;
            float gapBelowShelf = 5f;
            float templateInitY = _wsShelfDestRect.Y + _wsShelfDestRect.Height + gapBelowShelf;
            if (templateInitY + templateDestActualHeight > _screenHeight - 20f)
            {
                templateInitY = _screenHeight - 20f - templateDestActualHeight;
            }
            float minTemplateY = _wsShelfDestRect.Y + _wsShelfDestRect.Height + 15f;
            if (templateInitY < minTemplateY)
            {
                templateInitY = minTemplateY;
            }
            _wsTemplateDestRect = new Rectangle(
                (_screenWidth - templateDestActualWidth) / 2f, templateInitY,
                templateDestActualWidth, templateDestActualHeight);

            Rectangle tempInputBoxSource = new Rectangle(0, 0, 133, 30);
            Rectangle tempMeterAreaSource = new Rectangle(0, 0, 109, 99);


            float inputSlotRelXFactor = 0.030f;
            float inputSlotVisualDestWidth = tempInputBoxSource.Width * templateDisplayScale;
            float inputSlotVisualDestHeight = tempInputBoxSource.Height * templateDisplayScale;
            float[] inputSlotRelYsFactors = { 0.13f, 0.33f, 0.53f };

            _wsInputBoxVisualDestRects.Clear();
            _wsSelectedIngredientTextDestRects.Clear();
            for (int i = 0; i < MAX_SELECTED_INGREDIENTS; i++)
            {
                Rectangle visualDest = new Rectangle(
                    _wsTemplateDestRect.X + _wsTemplateDestRect.Width * inputSlotRelXFactor,
                    _wsTemplateDestRect.Y + _wsTemplateDestRect.Height * inputSlotRelYsFactors[i],
                    inputSlotVisualDestWidth, inputSlotVisualDestHeight);
                _wsInputBoxVisualDestRects.Add(visualDest);
                _wsSelectedIngredientTextDestRects.Add(new Rectangle(
                    visualDest.X + (5 * templateDisplayScale), visualDest.Y + (5 * templateDisplayScale),
                    visualDest.Width - (10 * templateDisplayScale), visualDest.Height - (10 * templateDisplayScale)));
            }

            _wsPropertyMeterAreaDestRect = new Rectangle(
                _wsTemplateDestRect.X + _wsTemplateDestRect.Width * 0.41f,
                _wsTemplateDestRect.Y + _wsTemplateDestRect.Height * 0.15f,
                tempMeterAreaSource.Width * templateDisplayScale,
                tempMeterAreaSource.Height * templateDisplayScale);

            string[] propertyNames = { "BITTER", "SWEET", "SOUR", "TOXIC" };
            _wsPropertyMeterBoxesDestRects.Clear();

            float desiredLabelBaseFontSize = 10f;
            float desiredSegmentBaseSquareSize = 8f;
            float basePaddingBelowLabel = 2f;
            float basePaddingBetweenMeterRows = 4f;

            float scaledLabelFontSize = Math.Max(7f, desiredLabelBaseFontSize * templateDisplayScale);
            float scaledSegmentSquareSize = Math.Max(4f, desiredSegmentBaseSquareSize * templateDisplayScale);
            float scaledPaddingBelowLabel = basePaddingBelowLabel * templateDisplayScale;
            float scaledPaddingBetweenMeterRows = basePaddingBetweenMeterRows * templateDisplayScale;

            float singleMeterRowTotalHeight = scaledLabelFontSize + scaledPaddingBelowLabel + scaledSegmentSquareSize;

            float allMetersBlockHeight = (propertyNames.Length * singleMeterRowTotalHeight) + Math.Max(0, propertyNames.Length - 1) * scaledPaddingBetweenMeterRows;
            float currentYWithinMeterArea = (_wsPropertyMeterAreaDestRect.Height - allMetersBlockHeight) / 2f;
            if (currentYWithinMeterArea < 0) currentYWithinMeterArea = 0;

            for (int p = 0; p < propertyNames.Length; p++)
            {
                string propName = propertyNames[p];
                _wsPropertyMeterBoxesDestRects[propName] = new List<Rectangle>();

                float labelRelativeY = currentYWithinMeterArea;
                float segmentsRelativeY = labelRelativeY + scaledLabelFontSize + scaledPaddingBelowLabel;
                float totalWidthOfSegmentsAndGaps = (NUM_METER_SEGMENTS * scaledSegmentSquareSize) + Math.Max(0, NUM_METER_SEGMENTS - 1) * (1 * templateDisplayScale);
                float segmentsRelativeStartX = (_wsPropertyMeterAreaDestRect.Width - totalWidthOfSegmentsAndGaps) / 2f;
                if (segmentsRelativeStartX < 0) segmentsRelativeStartX = 0;

                for (int i = 0; i < NUM_METER_SEGMENTS; i++)
                {
                    _wsPropertyMeterBoxesDestRects[propName].Add(new Rectangle(
                        _wsPropertyMeterAreaDestRect.X + segmentsRelativeStartX + i * (scaledSegmentSquareSize + (1 * templateDisplayScale)),
                        _wsPropertyMeterAreaDestRect.Y + segmentsRelativeY,
                        scaledSegmentSquareSize,
                        scaledSegmentSquareSize
                    ));
                }
                currentYWithinMeterArea += singleMeterRowTotalHeight + scaledPaddingBetweenMeterRows;
            }

            float newResetButtonSourceWidth = 103f;
            float newResetButtonSourceHeight = 35f;
            float newBrewButtonSourceWidth = 104f;
            float newBrewButtonSourceHeight = 35f;

            float buttonVisualScaleFactor = 0.85f;
            float buttonGroupYOffset = 15f * templateDisplayScale;

            float resetButtonDestW = newResetButtonSourceWidth * templateDisplayScale * buttonVisualScaleFactor;
            float resetButtonDestH = newResetButtonSourceHeight * templateDisplayScale * buttonVisualScaleFactor;
            float brewButtonDestW = newBrewButtonSourceWidth * templateDisplayScale * buttonVisualScaleFactor;
            float brewButtonDestH = newBrewButtonSourceHeight * templateDisplayScale * buttonVisualScaleFactor;

            float buttonPaddingFromMeterArea = 10f * templateDisplayScale;
            float verticalSpacingBetweenButtons = 5f * templateDisplayScale;

            _wsResetButtonDestRect = new Rectangle(
                _wsPropertyMeterAreaDestRect.X + _wsPropertyMeterAreaDestRect.Width + buttonPaddingFromMeterArea,
                _wsPropertyMeterAreaDestRect.Y + buttonGroupYOffset,
                resetButtonDestW,
                resetButtonDestH
            );

            _wsBrewButtonDestRect = new Rectangle(
                _wsPropertyMeterAreaDestRect.X + _wsPropertyMeterAreaDestRect.Width + buttonPaddingFromMeterArea,
                _wsResetButtonDestRect.Y + resetButtonDestH + verticalSpacingBetweenButtons,
                brewButtonDestW,
                brewButtonDestH
            );

            float animSlotSize = 100 * templateDisplayScale * 0.9f;
            float animSlotSpacing = 15 * templateDisplayScale;

            float totalAnimWidth = (MAX_SELECTED_INGREDIENTS * animSlotSize) + Math.Max(0, (MAX_SELECTED_INGREDIENTS - 1)) * animSlotSpacing;
            float animDestY = _wsTemplateDestRect.Y + _wsTemplateDestRect.Height + 20f;
            animDestY = _screenHeight * 0.65f;

            _wsBrewActionAnimDestRects.Clear();
            for (int i = 0; i < MAX_SELECTED_INGREDIENTS; i++)
            {
                float slotX = _wsTemplateDestRect.X + (_wsTemplateDestRect.Width - totalAnimWidth) / 2f + i * (animSlotSize + animSlotSpacing);
                Rectangle animRect = new Rectangle(slotX, animDestY, animSlotSize, animSlotSize);
                _wsBrewActionAnimDestRects.Add(animRect);
            }

            float servingBgDesiredWidth = _screenWidth * 0.45f;
            float servingBgScale = servingBgDesiredWidth / 232f;
            _wsServingAreaDestRect = new Rectangle
                                    (_screenWidth - (232f * servingBgScale) - 0,
                                    (_screenHeight - (349f * servingBgScale))
                                     / 2f, 232f * servingBgScale, 349f * servingBgScale);

            float cupScaleInArea = 0.23f;
            _wsCupDestRect = new Rectangle(_wsServingAreaDestRect.X + _wsServingAreaDestRect.Width * 0.43f,
                             _wsServingAreaDestRect.Y + _wsServingAreaDestRect.Height * 0.61f,
                             91f * servingBgScale * cupScaleInArea * 2.5f,
                             75f * servingBgScale * cupScaleInArea * 2.5f);

            float handScaleInArea = 0.7f;
            _wsHandDestRect = new Rectangle(_wsServingAreaDestRect.X + _wsServingAreaDestRect.Width * 0.40f,
                            _wsServingAreaDestRect.Y + _wsServingAreaDestRect.Height * 0.55f,
                            201f * servingBgScale * handScaleInArea,
                            200f * servingBgScale * handScaleInArea);
            _wsPreparedDrinkDestRect = new Rectangle(_wsCupDestRect.X + _wsCupDestRect.Width * 0.03f,
                                    _wsCupDestRect.Y + _wsCupDestRect.Height * -0.45f,
                                    _wsCupDestRect.Width * 1f,
                                    _wsCupDestRect.Height * 1f);

            float serveTrashBtnScale = 0.8f * servingBgScale;
            _wsServeButtonDestRect = new Rectangle(_wsServingAreaDestRect.X + _wsServingAreaDestRect.Width * 0.68f, _wsServingAreaDestRect.Y + _wsServingAreaDestRect.Height * 0.05f, 47f * serveTrashBtnScale, 47f * serveTrashBtnScale);
            _wsTrashButtonDestRect = new Rectangle(_wsServingAreaDestRect.X + _wsServingAreaDestRect.Width * 0.08f, _wsServingAreaDestRect.Y + _wsServingAreaDestRect.Height * 0.05f, 33f * serveTrashBtnScale, 38f * serveTrashBtnScale);

            _waterMeterBackgroundRect = new Rectangle(
                _screenWidth - WATER_METER_WIDTH - WATER_METER_SCREEN_OFFSET_X,
                WATER_METER_SCREEN_OFFSET_Y,
                WATER_METER_WIDTH,
                WATER_METER_HEIGHT
            );
            _useWaterJugButtonRect = new Rectangle(
                _waterMeterBackgroundRect.X + (_waterMeterBackgroundRect.Width / 2) - (WATER_JUG_ICON_SIZE / 2),
                _waterMeterBackgroundRect.Y + WATER_METER_HEIGHT + WATER_JUG_UI_OFFSET_Y,
                WATER_JUG_ICON_SIZE,
                WATER_JUG_ICON_SIZE
            );

            _currentWorkstationMachineContext = new InteractionContext(false, string.Empty, new List<string>());

            _refillWaterButtonRect = new Rectangle(
            _waterMeterBackgroundRect.X - REFILL_BUTTON_WIDTH - REFILL_BUTTON_OFFSET_X,
            _waterMeterBackgroundRect.Y + _waterMeterBackgroundRect.Height - REFILL_BUTTON_HEIGHT,
            REFILL_BUTTON_WIDTH,
            REFILL_BUTTON_HEIGHT
        );


            float trashCanSize = 50f;
            float trashCanPadding = 20f;
            _customerScreenTrashCanRect = new Rectangle(
                _screenWidth - trashCanSize - trashCanPadding,
                _screenHeight - trashCanSize - trashCanPadding - 50,
                trashCanSize,
                trashCanSize
            );

            _patienceMeterScreenPos = new Vector2(20, 150);
            _sanityMeterScreenPos = new Vector2(20, _patienceMeterScreenPos.Y + 40);

            _patienceMeterFrames = new Rectangle[6];
            _patienceMeterFrames = new Rectangle[MAX_PATIENCE_FRAMES];

            _sanityMeterCustomerScreenPos = new Vector2(20, 95);

            float lineSpacing = 25f;
            float meterPadding = 10f;

            float sanityMeterDisplayWidth = 363f;
            float sanityMeterDisplayHeight = 46f;
            float patienceMeterDisplayWidth = 48f;
            float patienceMeterDisplayHeight = 27f;

            float meterRightEdgeX = _screenWidth - WATER_METER_SCREEN_OFFSET_X - WATER_METER_WIDTH;
            meterRightEdgeX = _screenWidth - 20f;

            float topPadding = 20f;
            float verticalPaddingBetweenMeters = 10f;

            float topRightBlockX = _screenWidth - sanityMeterDisplayWidth - 20f;


            float hudStartY = 20f;
            float hudX = 20f;
            float meterAreaStartY = hudStartY + (lineSpacing * 3) + meterPadding;

            _sanityMeterScreenPos = new Vector2(
                meterRightEdgeX - sanityMeterDisplayWidth,
                topPadding
            );
            _sanityMeterHudPos = new Vector2(hudX, hudStartY + lineSpacing * 3 + 5);

            // 1. Sanity Meter Position
            _sanityMeterScreenPos = new Vector2(hudX, meterAreaStartY);

            float wsMeterStartX = 60f;
            float wsMeterStartY = 70f;

            _sanityMeterScreenPos = new Vector2(wsMeterStartX, wsMeterStartY);

            _patienceMeterScreenPos = new Vector2(
                meterRightEdgeX - patienceMeterDisplayWidth,
                _sanityMeterScreenPos.Y + sanityMeterDisplayHeight + verticalPaddingBetweenMeters
            );

            Vector2 borderScreenPos = _sanityMeterScreenPos;
            float meterRightScreenMargin = 20f;

            float bottomDesiredY = 140f;
            _patienceMeterScreenPos = new Vector2(hudX, bottomDesiredY - patienceMeterDisplayHeight);
            _sanityMeterScreenPos = new Vector2(hudX, _patienceMeterScreenPos.Y - meterPadding - sanityMeterDisplayHeight);

            _sanityMeterScreenPos = new Vector2(topRightBlockX, topPadding);
            _patienceMeterScreenPos = new Vector2(
                _screenWidth - patienceMeterDisplayWidth - meterRightScreenMargin,
                _sanityMeterScreenPos.Y + sanityMeterDisplayHeight + verticalPaddingBetweenMeters
            );

            if (_patienceMeterFrames == null || _patienceMeterFrames.Length != MAX_PATIENCE_FRAMES)
            {
                _patienceMeterFrames = new Rectangle[MAX_PATIENCE_FRAMES];
            }

            _masterVolume = Raylib.GetMasterVolume();
            _volumeSliderBarRect = new Rectangle(
                (_screenWidth - VOLUME_SLIDER_WIDTH) / 2f,
                VOLUME_SLIDER_POS_Y,
                VOLUME_SLIDER_WIDTH,
                VOLUME_SLIDER_HEIGHT
            );
            _volumeSliderKnobRect = new Rectangle(
                _volumeSliderBarRect.X + (_masterVolume * VOLUME_SLIDER_WIDTH) - VOLUME_KNOB_RADIUS,
                _volumeSliderBarRect.Y + (VOLUME_SLIDER_HEIGHT / 2f) - VOLUME_KNOB_RADIUS,
                VOLUME_KNOB_RADIUS * 2f,
                VOLUME_KNOB_RADIUS * 2f
            );
            _isDraggingVolumeKnob = false;

            _osMousePosition = Raylib.GetMousePosition();
            GameCursorPosition = _osMousePosition;
        }

        private Rectangle RectFromYourCoords(int x, int y, int endXVal, int endYVal)
        {
            return new Rectangle(x, y, Math.Max(1, endXVal - x), Math.Max(1, endYVal - y));
        }

        public void LoadAssets()
        {

            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string imagesPath = Path.Combine(basePath, "Images");
                string spritesheetsPath = Path.Combine(imagesPath, "Spritesheets");
                string workstationMiscPath = Path.Combine(imagesPath, "Workstation");

                try
                {
                    _patienceMeterTexture = Raylib.LoadTexture(Path.Combine(imagesPath, "patience_meter.png"));
                    if (_patienceMeterTexture.Id != 0)
                    {
                        Console.WriteLine("SUCCESS: Patience meter spritesheet loaded.");
                        float frameWidth = 48;
                        float frameHeight = 27;
                        float startY = 21;
                        for (int i = 0; i < 6; i++)
                        {
                            _patienceMeterFrames[i] = new Rectangle(i * frameWidth, startY, frameWidth, frameHeight);
                        }
                    }
                    else
                    {
                        Console.WriteLine("WARNING: Failed to load patience_meter.png.");
                    }

                    _sanityMeterTexture = Raylib.LoadTexture(Path.Combine(imagesPath, "sanity_meter.png"));
                    if (_sanityMeterTexture.Id != 0)
                    {
                        Console.WriteLine("SUCCESS: Sanity meter texture loaded.");
                        _sanityMeterBorderSourceRect = new Rectangle(16, 16, 363, 46);
                        _sanityMeterFillSourceBaseRect = new Rectangle(26, 68, 354, 80);
                    }
                    else
                    {
                        Console.WriteLine("WARNING: Failed to load sanity_meter.png.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ERROR loading new meter assets: {e.Message}");
                }

                try
                {
                    string customerImagesPath = Path.Combine(imagesPath, "Customer");

                    string hiddenToyolIconPath = Path.Combine(customerImagesPath, "toyol_static.png");
                    _hiddenToyolIconTexture = Raylib.LoadTexture(hiddenToyolIconPath);

                    if (_hiddenToyolIconTexture.Id != 0)
                    {
                        Console.WriteLine($"SUCCESS: Hidden Toyol icon loaded from '{hiddenToyolIconPath}'. Dimensions: W={_hiddenToyolIconTexture.Width}, H={_hiddenToyolIconTexture.Height}");
                    }
                    else
                    {
                        Console.WriteLine($"WARNING: Failed to load hidden Toyol icon from '{hiddenToyolIconPath}'. Hidden Toyol may not display correctly.");
                        _hiddenToyolIconTexture = new Texture2D { Id = 0 };
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ERROR loading hidden Toyol icon: {e.Message}");
                    _hiddenToyolIconTexture = new Texture2D { Id = 0 };
                }

                try
                {
                    _customCursorTexture = Raylib.LoadTexture(Path.Combine(imagesPath, "cursor.png"));
                    if (_customCursorTexture.Id == 0) Console.WriteLine("WARNING: Failed to load custom cursor.png");
                }
                catch (Exception e) { Console.WriteLine($"ERROR loading custom cursor: {e.Message}"); }

                try
                {
                    _heatwaveEffectTexture = Raylib.LoadTexture(Path.Combine(spritesheetsPath, "heatwave_spritesheet.png"));

                    if (_heatwaveEffectTexture.Id != 0)
                    {
                        Console.WriteLine("SUCCESS: Heatwave spritesheet loaded.");
                        Console.WriteLine($"Heatwave Texture Dimensions: W={_heatwaveEffectTexture.Width}, H={_heatwaveEffectTexture.Height}");


                        _heatwaveFrames = new Rectangle[HEATWAVE_TOTAL_FRAMES];

                        float frameWidth = (float)_heatwaveEffectTexture.Width / HEATWAVE_COLS;
                        float frameHeight = (float)_heatwaveEffectTexture.Height / HEATWAVE_ROWS;

                        _heatwaveFrameDuration = 1.0f / 15.0f;
                        _currentHeatwaveFrame = 0;
                        _heatwaveFrameTimer = 0f;

                        Console.WriteLine($"Heatwave: Calculated Frame W={frameWidth}, Frame H={frameHeight} for {HEATWAVE_COLS}x{HEATWAVE_ROWS} grid.");

                        int frameCounter = 0;
                        for (int y = 0; y < HEATWAVE_ROWS; y++)
                        {
                            for (int x = 0; x < HEATWAVE_COLS; x++)
                            {
                                if (frameCounter < HEATWAVE_TOTAL_FRAMES)
                                {
                                    _heatwaveFrames[frameCounter] = new Rectangle(
                                        x * frameWidth,    // X position of the frame in the spritesheet
                                        y * frameHeight,   // Y position of the frame in the spritesheet
                                        frameWidth,        // Width of one frame
                                        frameHeight        // Height of one frame
                                    );
                                    frameCounter++;
                                }
                            }
                        }

                        if (_heatwaveFrames.Length > 0)
                        {
                            Console.WriteLine($"Heatwave: First calculated sourceRect from new sheet: X={_heatwaveFrames[0].X:F0}, Y={_heatwaveFrames[0].Y:F0}, W={_heatwaveFrames[0].Width:F0}, H={_heatwaveFrames[0].Height:F0}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"WARNING: Failed to load heatwave spritesheet. Effect will not be shown.");
                        _heatwaveEffectTexture = new Texture2D { Id = 0 };
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ERROR loading heatwave effect: {e.Message}");
                    _heatwaveEffectTexture = new Texture2D { Id = 0 };
                }

                try
                {

                    _ghostScreenEffectTexture = Raylib.LoadTexture(Path.Combine(spritesheetsPath, "ghost_spritesheet.png"));

                    if (_ghostScreenEffectTexture.Id != 0)
                    {
                        Console.WriteLine($"SUCCESS: Ghost screen effect spritesheet loaded. ID: {_ghostScreenEffectTexture.Id}, Dimensions: W={_ghostScreenEffectTexture.Width}, H={_ghostScreenEffectTexture.Height}");

                        _ghostScreenEffectFrames = new Rectangle[GHOST_EFFECT_TOTAL_FRAMES];
                        float frameW = (float)_ghostScreenEffectTexture.Width / GHOST_EFFECT_COLS;
                        float frameH = (float)_ghostScreenEffectTexture.Height / GHOST_EFFECT_ROWS;

                        if (frameW <= 0 || frameH <= 0)
                        {
                            Console.WriteLine($"ERROR: Calculated ghost effect frame dimensions are invalid (W:{frameW}, H:{frameH}). Spritesheet dimensions or COLS/ROWS constants might be wrong.");
                            _ghostScreenEffectTexture = new Texture2D { Id = 0 };
                        }
                        else
                        {
                            Console.WriteLine($"Ghost Effect: Calculated Frame W={frameW:F2}, Frame H={frameH:F2} for {GHOST_EFFECT_COLS}x{GHOST_EFFECT_ROWS} grid.");
                            _ghostScreenEffectFrameDuration = 1.0f / 12.0f;
                            _currentGhostScreenEffectFrame = 0;
                            _ghostScreenEffectFrameTimer = 0f;

                            int frameCounter = 0;
                            for (int y = 0; y < GHOST_EFFECT_ROWS; y++)
                            {
                                for (int x = 0; x < GHOST_EFFECT_COLS; x++)
                                {
                                    if (frameCounter < GHOST_EFFECT_TOTAL_FRAMES)
                                    {
                                        _ghostScreenEffectFrames[frameCounter++] = new Rectangle(x * frameW, y * frameH, frameW, frameH);
                                    }
                                }
                            }
                            if (_ghostScreenEffectFrames.Length > 0 && frameCounter == GHOST_EFFECT_TOTAL_FRAMES)
                            {
                                Console.WriteLine($"Ghost Effect: Successfully populated {_ghostScreenEffectFrames.Length} frames. First frame: X={_ghostScreenEffectFrames[0].X:F0}, Y={_ghostScreenEffectFrames[0].Y:F0}, W={_ghostScreenEffectFrames[0].Width:F0}, H={_ghostScreenEffectFrames[0].Height:F0}");
                            }
                            else
                            {
                                Console.WriteLine($"ERROR: Ghost effect frame population issue. Counted: {frameCounter}, Expected: {GHOST_EFFECT_TOTAL_FRAMES}");
                                _ghostScreenEffectTexture = new Texture2D { Id = 0 };
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"WARNING: Failed to load 'ghost_spritesheet.png' from {spritesheetsPath}. Ghost screen effect will not be shown.");
                        _ghostScreenEffectTexture = new Texture2D { Id = 0 };
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ERROR loading ghost screen effect: {e.Message}");
                    _ghostScreenEffectTexture = new Texture2D { Id = 0 };
                }

                _mainFont = Raylib.GetFontDefault();

                _menuBackgroundTexture = Raylib.LoadTexture(Path.Combine(imagesPath, "MenuBackground.png"));
                if (_menuBackgroundTexture.Id == 0) Console.WriteLine("Warning: MenuBackground.png failed to load.");
                else { _menuBgFrameWidth = 1280f; _menuBgFrameHeight = 560f; _menuBgTotalFrames = 10; _menuBgCurrentFrame = 0; _menuBgFrameTimer = 0.0f; _menuBgFrameDuration = 1.0f / 12.0f; _menuBgSpriteSheetLoadedSuccessfully = true; }

                _nightCoffeeBgTexture = Raylib.LoadTexture(Path.Combine(imagesPath, "night_coffee.jpeg"));
                if (_nightCoffeeBgTexture.Id == 0) Console.WriteLine($"WARNING: Failed to load night_coffee.jpeg from {imagesPath}");

                _shopBackgroundTexture = Raylib.LoadTexture(Path.Combine(imagesPath, "shop_bg.jpeg"));
                if (_shopBackgroundTexture.Id == 0) Console.WriteLine("WARNING: Failed to load shop_bg.jpeg");


                _settingsIconTexture = Raylib.LoadTexture(Path.Combine(imagesPath, "settings.png"));
                if (_settingsIconTexture.Id == 0) Console.WriteLine("Warning: settings.png failed to load.");
                _customerScreenBgTexture = Raylib.LoadTexture(Path.Combine(imagesPath, "Customer_Screen.jpg"));
                if (_customerScreenBgTexture.Id == 0) Console.WriteLine("Warning: Customer_Screen.jpg failed to load.");
                _settingsScreenBgTexture = Raylib.LoadTexture(Path.Combine(imagesPath, "settings_background.jpeg"));
                if (_settingsScreenBgTexture.Id == 0) Console.WriteLine("Warning: settings_background.jpeg failed to load.");
                _levelIntroScreenBgTexture = Raylib.LoadTexture(Path.Combine(imagesPath, "LevelIntroScreen.jpeg"));
                if (_levelIntroScreenBgTexture.Id == 0) Console.WriteLine("Warning: LevelIntroScreen.jpeg failed to load.");
                _pauseButtonTexture = Raylib.LoadTexture(Path.Combine(imagesPath, "PauseButton.png"));
                if (_pauseButtonTexture.Id == 0) Console.WriteLine("Warning: PauseButton.png failed to load.");
                _dialogueBgTexture = Raylib.LoadTexture(Path.Combine(imagesPath, "dialogue_background.jpg"));
                if (_dialogueBgTexture.Id == 0) Console.WriteLine("Warning: dialogue_background.jpg failed to load.");
                _serveButtonTexture = Raylib.LoadTexture(Path.Combine(spritesheetsPath, "Serve.png"));
                if (_serveButtonTexture.Id == 0) Console.WriteLine($"WARNING: Failed to load Serve.png from {spritesheetsPath}");
                _trashButtonTexture = Raylib.LoadTexture(Path.Combine(spritesheetsPath, "Trash.png"));
                if (_trashButtonTexture.Id == 0) Console.WriteLine($"WARNING: Failed to load Trash.png from {spritesheetsPath}");
                _waterJugTexture = Raylib.LoadTexture(Path.Combine(spritesheetsPath, "water_jug.png"));
                if (_waterJugTexture.Id == 0) Console.WriteLine($"WARNING: Failed to load water_jug.png from {spritesheetsPath}");


                _workstationSpritesheet1 = Raylib.LoadTexture(Path.Combine(spritesheetsPath, "ct1.png"));
                _workstationSpritesheet2 = Raylib.LoadTexture(Path.Combine(spritesheetsPath, "ct2.png"));
                _blackBoxTexture = Raylib.LoadTexture(Path.Combine(workstationMiscPath, "black.png"));
                _whiteBoxTexture = Raylib.LoadTexture(Path.Combine(workstationMiscPath, "white.png"));
                _workstationButtonSpritesheet = Raylib.LoadTexture(Path.Combine(spritesheetsPath, "workstation_buttons.png"));


                if (_workstationSpritesheet1.Id == 0) Console.WriteLine($"CRITICAL ERROR: Failed to load ct1.png from {spritesheetsPath}");
                if (_workstationSpritesheet2.Id == 0) Console.WriteLine($"CRITICAL ERROR: Failed to load ct2.png from {spritesheetsPath}");
                if (_workstationButtonSpritesheet.Id == 0) Console.WriteLine($"CRITICAL ERROR: Failed to load workstation_buttons.png from {spritesheetsPath}");


                if (_workstationButtonSpritesheet.Id != 0)
                {
                    _wsResetButtonSourceRect = new Rectangle(337, 318, 103, 35);
                    _wsBrewButtonSourceRect = new Rectangle(337, 363, 104, 35);
                }

                _wsBackgroundSourceRect = RectFromYourCoords(0, 0, 722, 402);
                _wsShelfSourceRect = RectFromYourCoords(1, 404, 352, 500);
                _wsTemplateSourceRect = RectFromYourCoords(3, 502, 396, 678);
                _wsInputBoxSourceRect = RectFromYourCoords(372, 680, 504, 709);
                _wsPropertyMeterAreaSourceRect = RectFromYourCoords(504, 643, 612, 741);

                _wsIngredientNormalIconSourceRects[IngredientType.CoffeeBean] = RectFromYourCoords(353, 404, 432, 475);
                _wsIngredientHoverIconSourceRects[IngredientType.CoffeeBean] = RectFromYourCoords(434, 404, 517, 475);
                _wsIngredientNormalIconSourceRects[IngredientType.Milk] = RectFromYourCoords(852, 404, 909, 546);
                _wsIngredientHoverIconSourceRects[IngredientType.Milk] = RectFromYourCoords(910, 404, 970, 546);
                _wsIngredientNormalIconSourceRects[IngredientType.GreenTea] = RectFromYourCoords(518, 404, 560, 480);
                _wsIngredientHoverIconSourceRects[IngredientType.GreenTea] = RectFromYourCoords(562, 404, 608, 480);
                _wsIngredientNormalIconSourceRects[IngredientType.Tea] = RectFromYourCoords(609, 404, 665, 453);
                _wsIngredientHoverIconSourceRects[IngredientType.Tea] = RectFromYourCoords(666, 404, 725, 453);
                _wsIngredientNormalIconSourceRects[IngredientType.Chocolate] = RectFromYourCoords(726, 404, 786, 457);
                _wsIngredientHoverIconSourceRects[IngredientType.Chocolate] = RectFromYourCoords(787, 404, 850, 457);
                _wsIngredientNormalIconSourceRects[IngredientType.Ginger] = RectFromYourCoords(397, 481, 454, 525);
                _wsIngredientHoverIconSourceRects[IngredientType.Ginger] = RectFromYourCoords(455, 481, 514, 527);
                _wsIngredientNormalIconSourceRects[IngredientType.Mint] = RectFromYourCoords(516, 481, 582, 545);
                _wsIngredientHoverIconSourceRects[IngredientType.Mint] = RectFromYourCoords(583, 481, 652, 547);
                _wsIngredientNormalIconSourceRects[IngredientType.Lemon] = RectFromYourCoords(653, 481, 713, 529);
                _wsIngredientHoverIconSourceRects[IngredientType.Lemon] = RectFromYourCoords(714, 481, 776, 531);
                _wsIngredientNormalIconSourceRects[IngredientType.Honey] = RectFromYourCoords(397, 548, 441, 597);
                _wsIngredientHoverIconSourceRects[IngredientType.Honey] = RectFromYourCoords(442, 548, 489, 600);
                _wsIngredientNormalIconSourceRects[IngredientType.Cinnamon] = RectFromYourCoords(490, 548, 550, 608);
                _wsIngredientHoverIconSourceRects[IngredientType.Cinnamon] = RectFromYourCoords(551, 548, 613, 610);

                _wsIngredientActionAnimSourceRects[IngredientType.CoffeeBean] = RectFromYourCoords(1, 754, 129, 921);
                _wsIngredientActionAnimSourceRects[IngredientType.Lemon] = RectFromYourCoords(1, 922, 129, 1089);
                _wsIngredientActionAnimSourceRects[IngredientType.GreenTea] = RectFromYourCoords(130, 754, 257, 921);
                _wsIngredientActionAnimSourceRects[IngredientType.Honey] = RectFromYourCoords(130, 922, 258, 1089);
                _wsIngredientActionAnimSourceRects[IngredientType.Tea] = RectFromYourCoords(259, 754, 386, 921);
                _wsIngredientActionAnimSourceRects[IngredientType.Cinnamon] = RectFromYourCoords(259, 922, 387, 1089);
                _wsIngredientActionAnimSourceRects[IngredientType.Chocolate] = RectFromYourCoords(388, 754, 516, 921);
                _wsIngredientActionAnimSourceRects[IngredientType.Milk] = RectFromYourCoords(517, 754, 645, 921);
                _wsIngredientActionAnimSourceRects[IngredientType.Ginger] = RectFromYourCoords(646, 754, 774, 921);
                _wsIngredientActionAnimSourceRects[IngredientType.Mint] = RectFromYourCoords(775, 754, 903, 921);

                _wsDrinkOutputSourceRects["Ginger Latte"] = RectFromYourCoords(291, 0, 388, 65);
                _wsDrinkOutputSourceRects["Zesty Coffee"] = RectFromYourCoords(388, 0, 485, 65);
                _wsDrinkOutputSourceRects["Pooh Coffee"] = RectFromYourCoords(485, 0, 582, 65);
                _wsDrinkOutputSourceRects["Latte"] = RectFromYourCoords(582, 0, 680, 65);
                _wsDrinkOutputSourceRects["Honey Lemon"] = RectFromYourCoords(873, 0, 970, 65);
                _wsDrinkOutputSourceRects["Espresso"] = RectFromYourCoords(97, 75, 194, 140);
                _wsDrinkOutputSourceRects["Chocolate Milk"] = RectFromYourCoords(194, 75, 291, 140);
                _wsDrinkOutputSourceRects["Green Milk Tea"] = RectFromYourCoords(582, 75, 679, 140);
                _wsDrinkOutputSourceRects["Mint"] = RectFromYourCoords(776, 75, 873, 140);
                _wsDrinkOutputSourceRects["Classic Tea"] = RectFromYourCoords(97, 150, 194, 214);
                _wsDrinkOutputSourceRects["Lemon Tea"] = RectFromYourCoords(194, 150, 291, 214);
                _wsDrinkOutputSourceRects["Cinnamon Honey Tea"] = RectFromYourCoords(388, 150, 485, 214);
                _wsDrinkOutputSourceRects["Green Tea"] = RectFromYourCoords(582, 150, 678, 214);
                _wsDrinkOutputSourceRects["Cinnamon Coffee"] = RectFromYourCoords(776, 150, 873, 214);
                _wsDrinkOutputSourceRects["Milk Tea"] = RectFromYourCoords(97, 225, 194, 290);
                _wsDrinkOutputSourceRects["Firey Brew"] = RectFromYourCoords(0, 225, 97, 290);
                _wsDrinkOutputSourceRects["Shadow Brew"] = RectFromYourCoords(0, 75, 97, 140);
                _wsDrinkOutputSourceRects["Muddled Mess"] = RectFromYourCoords(2, 1, 97, 64);
                _wsDrinkOutputSourceRects["Alien Concoction"] = RectFromYourCoords(2, 152, 95, 213);

                _wsCupSourceRect = RectFromYourCoords(0, 300, 91, 375);
                _wsHandSourceRect = RectFromYourCoords(112, 300, 313, 500);
                _wsServingBgSourceRect = RectFromYourCoords(676, 300, 908, 649);
                _wsTrashIconSourceRect = RectFromYourCoords(514, 300, 547, 338);
                _wsServeIconSourceRect = RectFromYourCoords(580, 300, 627, 347);
                System.Console.WriteLine("UIManager: Asset loading attempt finished.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR loading UIManager assets: {e.GetType().Name} - {e.Message}\n{e.StackTrace}");
                if (_mainFont.Texture.Id == 0) _mainFont = Raylib.GetFontDefault();
            }
        }

        public void UnloadAssets()
        {
            if (_mainFont.Texture.Id != 0 && _mainFont.Texture.Id != Raylib.GetFontDefault().Texture.Id) Raylib.UnloadFont(_mainFont);
            if (_menuBackgroundTexture.Id != 0) Raylib.UnloadTexture(_menuBackgroundTexture);
            if (_nightCoffeeBgTexture.Id != 0) Raylib.UnloadTexture(_nightCoffeeBgTexture);
            if (_shopBackgroundTexture.Id != 0) Raylib.UnloadTexture(_shopBackgroundTexture);
            if (_settingsIconTexture.Id != 0) Raylib.UnloadTexture(_settingsIconTexture);
            if (_customerScreenBgTexture.Id != 0) Raylib.UnloadTexture(_customerScreenBgTexture);
            if (_settingsScreenBgTexture.Id != 0) Raylib.UnloadTexture(_settingsScreenBgTexture);
            if (_levelIntroScreenBgTexture.Id != 0) Raylib.UnloadTexture(_levelIntroScreenBgTexture);
            if (_pauseButtonTexture.Id != 0) Raylib.UnloadTexture(_pauseButtonTexture);
            if (_dialogueBgTexture.Id != 0) Raylib.UnloadTexture(_dialogueBgTexture);
            if (_workstationSpritesheet1.Id != 0) Raylib.UnloadTexture(_workstationSpritesheet1);
            if (_workstationSpritesheet2.Id != 0) Raylib.UnloadTexture(_workstationSpritesheet2);
            if (_blackBoxTexture.Id != 0) Raylib.UnloadTexture(_blackBoxTexture);
            if (_whiteBoxTexture.Id != 0) Raylib.UnloadTexture(_whiteBoxTexture);
            if (_workstationButtonSpritesheet.Id != 0) Raylib.UnloadTexture(_workstationButtonSpritesheet);
            if (_serveButtonTexture.Id != 0) Raylib.UnloadTexture(_serveButtonTexture);
            if (_trashButtonTexture.Id != 0) Raylib.UnloadTexture(_trashButtonTexture);
            if (_waterJugTexture.Id != 0) Raylib.UnloadTexture(_waterJugTexture);
            if (_heatwaveEffectTexture.Id != 0) Raylib.UnloadTexture(_heatwaveEffectTexture);
            if (_hiddenToyolIconTexture.Id != 0) Raylib.UnloadTexture(_hiddenToyolIconTexture);
            if (_patienceMeterTexture.Id != 0) Raylib.UnloadTexture(_patienceMeterTexture);
            if (_sanityMeterTexture.Id != 0) Raylib.UnloadTexture(_sanityMeterTexture);
            Console.WriteLine("UIManager: Assets unloaded.");
        }

        public void StartWorkstationBrewingAnimations()
        {
            if (_currentGameView == GameView.WorkstationScreen)
            {
                _currentWorkstationPhase = WorkstationPhase.AnimatingBrewActions;
                _brewActionAnimationTimer = 0f;
                _currentBrewActionIndex = 0;
                Console.WriteLine("UIManager: Workstation phase changed to AnimatingBrewActions.");
            }
        }

        public void ShowWorkstationServeTrashUI(bool brewSuccess, Drink? preparedDrink, string message)
        {
            if (_currentGameView == GameView.WorkstationScreen || _currentWorkstationPhase == WorkstationPhase.AnimatingBrewActions)
            {
                _workstationBrewSuccess = brewSuccess;
                _workstationPreparedDrink = preparedDrink;
                SetWorkstationMessage(message, !brewSuccess);
                _currentWorkstationPhase = WorkstationPhase.AwaitingServeOrTrashChoice;
                Console.WriteLine($"UIManager: Workstation phase changed to AwaitingServeOrTrashChoice. Brew success: {brewSuccess}");
            }
        }

        public void ResetWorkstationToSelection(GameManager gameManager)
        {
            _currentWorkstationPhase = WorkstationPhase.SelectingIngredients;
            _workstationMessage = string.Empty;
            _workstationPreparedDrink = null;
            _brewActionAnimationTimer = 0f;
            _currentBrewActionIndex = 0;

            if (gameManager != null && gameManager.Player != null)
            {
                gameManager.Player.SelectedIngredientsForBrewing.Clear();
                Console.WriteLine("[UIManager.ResetWorkstationToSelection] Player's selected ingredients for brewing CLEARED.");
            }
            else
            {
                Console.WriteLine("[UIManager.ResetWorkstationToSelection] WARNING: GameManager instance was null. Could not clear selected ingredients.");
            }
            Console.WriteLine("UIManager: Workstation phase reset to SelectingIngredients. UI elements cleared.");
        }

        private void SetWorkstationMessage(string message, bool isError)
        {
            _workstationMessage = message;
            _workstationMessageIsError = isError;
            _workstationMessageTimer = WORKSTATION_MESSAGE_DURATION;
        }

        public void Update(float deltaTime, GameManager gameManager)
        {
            _osMousePosition = Raylib.GetMousePosition();
            float currentLerpSpeed;
            _gameManagerInstance = gameManager;

            if (gameManager.IsGhostInterferenceActive && gameManager.CursorSlowFactor < 1.0f)
            {
                currentLerpSpeed = GHOST_CURSOR_LERP_BASE_SPEED * gameManager.CursorSlowFactor;

                currentLerpSpeed = 0.02f + (gameManager.CursorSlowFactor * 0.1f);
                currentLerpSpeed = Math.Clamp(currentLerpSpeed, 0.01f, NORMAL_CURSOR_LERP_SPEED);
                //Console.WriteLine($"[UIManager.Update] Ghost cursor slowdown ACTIVE. LerpSpeed: {currentLerpSpeed}, Factor: {gameManager.CursorSlowFactor}");
            }
            else
            {
                currentLerpSpeed = NORMAL_CURSOR_LERP_SPEED;
            }
            GameCursorPosition = System.Numerics.Vector2.Lerp(GameCursorPosition, _osMousePosition, currentLerpSpeed);

            if (_menuBgSpriteSheetLoadedSuccessfully && _currentGameView == GameView.MainMenuScreen)
            {
                _menuBgFrameTimer += deltaTime;
                if (_menuBgFrameTimer >= _menuBgFrameDuration)
                {
                    _menuBgFrameTimer -= _menuBgFrameDuration;
                    _menuBgCurrentFrame = (_menuBgCurrentFrame + 1) % _menuBgTotalFrames;
                }
            }

            if (_workstationMessageTimer > 0)
            {
                _workstationMessageTimer -= deltaTime;
                if (_workstationMessageTimer <= 0)
                {
                    _workstationMessage = string.Empty;
                    _workstationMessageIsError = false;
                }
            }

            if (gameManager.GameState == GameState.MainMenu) UpdateMainMenuInput(gameManager);
            else if (gameManager.GameState == GameState.Playing)
            {
                switch (gameManager.CurrentGameView)
                {
                    case GameView.CustomerScreen: UpdateInGameUIInput(gameManager); break;
                    case GameView.DialogueScreen: UpdateDialogueScreenInput(gameManager, deltaTime); break;
                    case GameView.WorkstationScreen: UpdateWorkstationScreenInput(gameManager, deltaTime); break;
                }
            }
            else if (gameManager.GameState == GameState.Paused)
            {
                if (gameManager.CurrentGameView == GameView.ShopScreen) UpdateShopScreenInput(gameManager);
                else if (gameManager.CurrentGameView == GameView.SettingsScreen) UpdateSettingsScreenInput(gameManager);
                else UpdatePauseMenuInput(gameManager);
            }
            else if (gameManager.GameState == GameState.GameOver || gameManager.GameState == GameState.GameComplete)
            {
                if (gameManager.GameWasWon) UpdateGameCompleteScreenInput(gameManager);
                else UpdateGameOverInput(gameManager);
            }
            else if (gameManager.GameState == GameState.GameOver)
            {
                UpdateGameOverInput(gameManager);
            }

            if (_currentGameView == GameView.SettingsScreen)
            {
                UpdateSettingsScreenInput(gameManager);
            }

            if (_heatwaveEffectTexture.Id != 0 && gameManager.GameState == GameState.Playing &&
                (gameManager.CurrentGameView == GameView.CustomerScreen || gameManager.CurrentGameView == GameView.DialogueScreen))
            {
                bool fireMonsterPresent = gameManager.ShopLayout.CustomerQueue.Any(cust => cust is SupernaturalCustomer sc && sc.Type == SupernaturalCustomerType.FireMonster) ||
                                        (gameManager.CurrentlyInteractingCustomer is SupernaturalCustomer sci && sci.Type == SupernaturalCustomerType.FireMonster);

                if (fireMonsterPresent)
                {
                    _heatwaveFrameTimer += deltaTime;
                    if (_heatwaveFrameTimer >= _heatwaveFrameDuration)
                    {
                        _heatwaveFrameTimer = 0f;
                        _currentHeatwaveFrame = (_currentHeatwaveFrame + 1) % HEATWAVE_TOTAL_FRAMES;
                    }
                }
            }
            if (_ghostScreenEffectTexture.Id != 0 && gameManager.GameState == GameState.Playing &&
                (gameManager.CurrentGameView == GameView.CustomerScreen || gameManager.CurrentGameView == GameView.DialogueScreen))
            {
                bool ghostIsPresentQuery = gameManager.ShopLayout.CustomerQueue.Any(cust => cust is SupernaturalCustomer sc && sc.Type == SupernaturalCustomerType.Ghost) ||
                                        (gameManager.CurrentlyInteractingCustomer is SupernaturalCustomer sci && sci.Type == SupernaturalCustomerType.Ghost);

                if (ghostIsPresentQuery || gameManager.IsGhostInterferenceActive)
                {

                    //Console.WriteLine($"[UIManager.Update] Animating ghost screen effect. GhostPresent: {ghostIsPresentQuery}, InterferenceActive: {gameManager.IsGhostInterferenceActive}, CurrentFrame: {_currentGhostScreenEffectFrame}");
                    _ghostScreenEffectFrameTimer += deltaTime;
                    if (_ghostScreenEffectFrameTimer >= _ghostScreenEffectFrameDuration)
                    {
                        _ghostScreenEffectFrameTimer = 0f;
                        _currentGhostScreenEffectFrame = (_currentGhostScreenEffectFrame + 1) % GHOST_EFFECT_TOTAL_FRAMES;
                    }
                }
            }
        }

        private void UpdateGameCompleteScreenInput(GameManager gameManager)
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                Vector2 mousePos = this.GameCursorPosition;
                if (Raylib.CheckCollisionPointRec(mousePos, _gameCompleteRestartButtonRect))
                {
                    gameManager.StartGame();
                }
                else if (Raylib.CheckCollisionPointRec(mousePos, _gameCompleteMenuButtonRect))
                {
                    gameManager.ReturnToMainMenu();
                }
            }
        }

        private void UpdateMainMenuInput(GameManager gameManager)
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                Vector2 mousePosition = this.GameCursorPosition;
                if (Raylib.CheckCollisionPointRec(mousePosition, _playButtonDest)) gameManager.StartGame();
                else if (Raylib.CheckCollisionPointRec(mousePosition, _exitButtonDest)) gameManager.RequestQuit();
                else if (Raylib.CheckCollisionPointRec(mousePosition, _settingsIconDestRect)) gameManager.OpenSettings();
            }
        }

        private void UpdateInGameUIInput(GameManager gameManager)
        {
            Vector2 mousePosition = this.GameCursorPosition;
            if (Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                if (gameManager.IsToyolHiding &&
                    gameManager.CurrentGameView == GameView.CustomerScreen &&
                    Raylib.CheckCollisionPointRec(mousePosition, gameManager.HiddenToyolClickRect))
                {
                    Console.WriteLine("[UIManager.UpdateInGameUIInput] Hidden Toyol clicked!");
                    gameManager.PlayerFoundHiddenToyol();
                    return;
                }
                if (gameManager.CurrentGameView == GameView.CustomerScreen && Raylib.CheckCollisionPointRec(mousePosition, _pauseButtonScreenRect))
                {
                    gameManager.PauseGame();
                    return;
                }

                if (Raylib.CheckCollisionPointRec(mousePosition, _shopAccessButtonDest))
                {
                    Console.WriteLine("UIManager: Shop button clicked.");
                    gameManager.OpenShop();
                    return;
                }

                if (gameManager.CurrentGameView == GameView.CustomerScreen && _customerClickAreas.Any())
                {
                    for (int i = 0; i < _customerClickAreas.Count; i++)
                    {
                        var customerAreaTuple = _customerClickAreas[i];
                        if (Raylib.CheckCollisionPointRec(mousePosition, customerAreaTuple.Item2))
                        {
                            Customer clickedCustomer = customerAreaTuple.Item1;
                            if (gameManager.Player.HeldItem is Drink drinkToServe &&
                                gameManager.Player.CurrentOrder != null &&
                                gameManager.Player.CurrentOrder.RequestingCustomer == clickedCustomer &&
                                gameManager.Player.CurrentOrder.CurrentStatus == OrderStatus.Ready)
                            {
                                gameManager.Player.ServeDrink(clickedCustomer, gameManager);
                            }
                            else if ((gameManager.Player.CurrentOrder == null || gameManager.Player.CurrentOrder.RequestingCustomer != clickedCustomer) &&
                                     (clickedCustomer.CurrentState == CustomerState.Waiting || clickedCustomer.CurrentState == CustomerState.Ordering))
                            {
                                gameManager.InitiateInteractionWithCustomer(clickedCustomer);
                            }
                            else if (gameManager.Player.CurrentOrder != null && gameManager.Player.CurrentOrder.RequestingCustomer != clickedCustomer)
                            {
                                this.DisplayMessage("Finish serving your current order first!", MessageType.Warning, true);
                            }
                            else if (gameManager.Player.HeldItem == null && gameManager.Player.CurrentOrder?.RequestingCustomer == clickedCustomer && gameManager.Player.CurrentOrder.CurrentStatus == OrderStatus.Ready)
                            {
                                this.DisplayMessage("You need to pick up the drink from the workstation!", MessageType.Info, true);
                            }
                            break;
                        }
                    }
                }

                if (gameManager.Player.HeldItem is Drink &&
                    Raylib.CheckCollisionPointRec(mousePosition, _customerScreenTrashCanRect))
                {
                    Console.WriteLine("[UIManager.UpdateInGameUIInput] Customer screen trash can clicked.");
                    gameManager.PlayerDisposedHeldItemOnScreen();
                    return;
                }
            }
        }

        private void UpdateDialogueScreenInput(GameManager gameManager, float deltaTime)
        {
            if (gameManager.CurrentlyInteractingCustomer == null || gameManager.CurrentLevel == null)
            {
                return;
            }

            Customer interactingCustomer = gameManager.CurrentlyInteractingCustomer;
            Dialogue currentDialogue = interactingCustomer.DialogueSet;

            _dialoguePortraitFrameTimer += deltaTime;
            if (_dialoguePortraitFrameTimer >= _dialoguePortraitFrameDuration)
            {
                _dialoguePortraitFrameTimer -= _dialoguePortraitFrameDuration;
                interactingCustomer.CycleDialogueFrame();
            }

            bool dialogueShouldConclude = false;
            bool orderWasSuccessfullyTakenOrEstablished = (gameManager.Player.CurrentOrder != null && gameManager.Player.CurrentOrder.RequestingCustomer == interactingCustomer);

            if (Raylib.IsMouseButtonReleased(MouseButton.Left) || Raylib.IsKeyPressed(KeyboardKey.Enter))
            {
                Console.WriteLine($"[UIManager.UpdateDialogueScreenInput] Click/Enter. CurrentLineIndex: {currentDialogue.CurrentLineIndex}, HasMoreLines: {currentDialogue.HasMoreLines()}");

                if (interactingCustomer is SupernaturalCustomer sc && sc.Type == SupernaturalCustomerType.Alien &&
                    (interactingCustomer.Order == null || !interactingCustomer.Order.AlienDemand.HasValue))
                {
                    Console.WriteLine($"[UIManager.UpdateDialogueScreenInput] Interacting with Alien. Attempting to generate property demand via Player.TakeOrder.");
                    Order? alienOrder = gameManager.Player.TakeOrder(interactingCustomer, gameManager.CurrentLevel.AvailableRecipes, gameManager.GetCurrentLevelNumber(), gameManager);
                    orderWasSuccessfullyTakenOrEstablished = (alienOrder != null && alienOrder.AlienDemand.HasValue);
                    if (orderWasSuccessfullyTakenOrEstablished)
                    {
                        Console.WriteLine($"[UIManager.UpdateDialogueScreenInput] Alien demand order established by Player.TakeOrder.");
                        currentDialogue.AdvanceToNextLine();
                    }
                    else
                    {
                        Console.WriteLine($"[UIManager.UpdateDialogueScreenInput] Alien Player.TakeOrder did not establish a demand order. This is unusual.");
                        dialogueShouldConclude = true;
                    }
                }
                else if (!currentDialogue.AdvanceToNextLine())
                {
                    Console.WriteLine($"[UIManager.UpdateDialogueScreenInput] End of pre-set dialogue lines reached for {interactingCustomer.CustomerID}.");
                    if (!orderWasSuccessfullyTakenOrEstablished)
                    {
                        Console.WriteLine($"[UIManager.UpdateDialogueScreenInput] Attempting final Player.TakeOrder for {interactingCustomer.CustomerID}.");
                        Order? finalOrder = gameManager.Player.TakeOrder(interactingCustomer, gameManager.CurrentLevel.AvailableRecipes, gameManager.GetCurrentLevelNumber(), gameManager);
                        orderWasSuccessfullyTakenOrEstablished = (finalOrder != null);
                    }
                    dialogueShouldConclude = true;
                }

                if (dialogueShouldConclude)
                {
                    Console.WriteLine($"[UIManager.UpdateDialogueScreenInput] Dialogue concluded for {interactingCustomer.CustomerID}. OrderTaken/Established: {orderWasSuccessfullyTakenOrEstablished}");
                    gameManager.ConcludeInteractionAfterDialogue(orderTaken: orderWasSuccessfullyTakenOrEstablished);
                }
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                Console.WriteLine("[UIManager.UpdateDialogueScreenInput] Escape pressed, concluding dialogue (marking as no order taken).");
                gameManager.ConcludeInteractionAfterDialogue(orderTaken: false);
            }
        }


        private void UpdateWorkstationScreenInput(GameManager gameManager, float deltaTime)
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            bool canProcessClick = (Raylib.GetTime() - _lastClickProcessedTime) >= CLICK_COOLDOWN_DURATION;

            if (_workstationMessageTimer <= 0 && !string.IsNullOrEmpty(_workstationMessage))
            {
                Console.WriteLine("[UIManager.UpdateWorkstationScreenInput] Defensive clear (top): Clearing stale workstation message.");
                _workstationMessage = string.Empty;
                _workstationMessageIsError = false;
            }

            Equipment? currentEquipment = gameManager.ShopLayout.GetStation(EquipmentType.CoffeeMachine)?.AssignedEquipment;
            CoffeeMachine? coffeeMachine = currentEquipment as CoffeeMachine;

            if (coffeeMachine != null)
            {
                _currentWorkstationMachineContext = coffeeMachine.InitiateInteraction();
            }
            else
            {
                _currentWorkstationMachineContext = new InteractionContext(false, "Coffee machine not found", new List<string>());
            }


            if (gameManager.IsBrewingStationOverheated)
            {
                if (gameManager.Player.HasWaterJug()) // Check if player has a jug to use
                {
                    if (Raylib.CheckCollisionPointRec(mousePos, _useWaterJugButtonRect) && Raylib.IsMouseButtonReleased(MouseButton.Left))
                    {
                        if (canProcessClick)
                        {
                            _lastClickProcessedTime = (float)Raylib.GetTime();
                            Console.WriteLine("UIManager: 'Use Water Jug' asset/button clicked while overheated.");
                            bool cooled = gameManager.AttemptToCoolStation(); // This will consume a jug and set IsBrewingStationOverheated to false if successful
                            if (cooled)
                            {

                            }
                        }
                    }
                }
                return;
            }

            if (!gameManager.IsBrewingStationOverheated && coffeeMachine != null && _currentWorkstationMachineContext.AvailableActions.Contains("Refill Water"))
            {
                if (Raylib.CheckCollisionPointRec(mousePos, _refillWaterButtonRect) && Raylib.IsMouseButtonReleased(MouseButton.Left))
                {
                    if (canProcessClick)
                    {
                        _lastClickProcessedTime = (float)Raylib.GetTime();
                        Console.WriteLine("UIManager: 'Refill Water' button clicked.");
                        gameManager.AttemptRefillCoffeeMachineWater();

                        _currentWorkstationMachineContext = coffeeMachine.InitiateInteraction();
                    }
                }
            }

            switch (_currentWorkstationPhase)
            {
                case WorkstationPhase.SelectingIngredients:

                    for (int i = 0; i < _wsIngredientDisplayDestRectsOnShelf.Count && i < _displayableIngredientTypesOnShelf.Count; i++)
                    {
                        if (Raylib.CheckCollisionPointRec(mousePos, _wsIngredientDisplayDestRectsOnShelf[i]))
                        {
                            if (Raylib.IsMouseButtonReleased(MouseButton.Left))
                            {
                                if (canProcessClick)
                                {
                                    _lastClickProcessedTime = (float)Raylib.GetTime();

                                    IngredientType typeToAdd = _displayableIngredientTypesOnShelf[i];

                                    if (gameManager.IsIngredientStolen(typeToAdd))
                                    {
                                        SetWorkstationMessage($"Cannot select {typeToAdd.ToString()}! It's been stolen!", true);
                                    }
                                    else
                                    {
                                        if (gameManager.Player.SelectedIngredientsForBrewing.Count < MAX_SELECTED_INGREDIENTS)
                                        {
                                            Ingredient? ingredientInstance = gameManager.CurrentLevel?.UnlockedIngredients.FirstOrDefault(ing => ing.Type == typeToAdd);

                                            if (ingredientInstance != null)
                                            {
                                                Console.WriteLine($"--- Ingredient Selection Debug ---");
                                                Console.WriteLine($"Attempting to add: {typeToAdd}");
                                                Console.WriteLine($"SelectedIngredientsForBrewing (Count: {gameManager.Player.SelectedIngredientsForBrewing.Count}):");
                                                foreach (var selIng in gameManager.Player.SelectedIngredientsForBrewing)
                                                {
                                                    Console.WriteLine($"- {selIng.Name} (Type: {selIng.Type})");
                                                }

                                                bool typeAlreadySelected = gameManager.Player.SelectedIngredientsForBrewing.Any(selIng => selIng.Type == typeToAdd);

                                                Console.WriteLine($"Result of typeAlreadySelected check: {typeAlreadySelected}");
                                                Console.WriteLine($"--- End Ingredient Selection Debug ---");


                                                if (!typeAlreadySelected)
                                                {
                                                    gameManager.Player.SelectedIngredientsForBrewing.Add(ingredientInstance);
                                                    SetWorkstationMessage($"Added {ingredientInstance.Name}.", false);
                                                }
                                                else
                                                {
                                                    SetWorkstationMessage($"{ingredientInstance.Name} (type) already selected!", true);
                                                }
                                            }
                                            else
                                            {
                                                SetWorkstationMessage($"Error: {typeToAdd.ToString()} is not yet unlocked!", true);
                                            }
                                        }
                                        else
                                        {
                                            SetWorkstationMessage("Max 3 ingredients selected!", true);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (Raylib.CheckCollisionPointRec(mousePos, _wsResetButtonDestRect) && Raylib.IsMouseButtonReleased(MouseButton.Left))
                    {
                        if (canProcessClick)
                        {
                            _lastClickProcessedTime = (float)Raylib.GetTime();
                            Console.WriteLine("UIManager: Reset Button Clicked.");
                            gameManager.Player.SelectedIngredientsForBrewing.Clear();
                            SetWorkstationMessage("Selection cleared.", false);
                            _currentBrewActionIndex = 0;
                            _brewActionAnimationTimer = 0f;
                        }
                    }

                    if (Raylib.CheckCollisionPointRec(mousePos, _wsBrewButtonDestRect) && Raylib.IsMouseButtonReleased(MouseButton.Left))
                    {
                        if (canProcessClick)
                        {
                            _lastClickProcessedTime = (float)Raylib.GetTime();
                            Console.WriteLine("UIManager: Brew Button Clicked.");
                            if (gameManager.Player.SelectedIngredientsForBrewing.Any())
                            {
                                if (gameManager.Player.CurrentOrder != null)
                                {
                                    gameManager.InitiateBrewingProcess();
                                }
                                else
                                {
                                    SetWorkstationMessage("No active customer order to brew for!", true);
                                }
                            }
                            else
                            {
                                SetWorkstationMessage("Please select some ingredients first!", true);
                            }
                        }
                    }
                    break;

                case WorkstationPhase.AnimatingBrewActions:
                    _brewActionAnimationTimer += deltaTime;
                    if (_brewActionAnimationTimer >= BREW_ACTION_ANIM_DURATION_PER_STEP)
                    {
                        _brewActionAnimationTimer = 0f;
                        _currentBrewActionIndex++;
                        if (gameManager.Player.SelectedIngredientsForBrewing.Count == 0 ||
                            _currentBrewActionIndex >= gameManager.Player.SelectedIngredientsForBrewing.Count)
                        {
                            gameManager.AllBrewingAnimationsComplete();
                        }
                    }
                    break;

                case WorkstationPhase.AwaitingServeOrTrashChoice:
                    if (Raylib.CheckCollisionPointRec(mousePos, _wsServeButtonDestRect) && Raylib.IsMouseButtonReleased(MouseButton.Left))
                    {
                        if (canProcessClick)
                        {
                            _lastClickProcessedTime = (float)Raylib.GetTime();
                            Console.WriteLine("UIManager: Serve Button Clicked.");
                            gameManager.PlayerConfirmedServe();
                        }
                    }
                    if (Raylib.CheckCollisionPointRec(mousePos, _wsTrashButtonDestRect) && Raylib.IsMouseButtonReleased(MouseButton.Left))
                    {
                        if (canProcessClick)
                        {
                            _lastClickProcessedTime = (float)Raylib.GetTime();
                            Console.WriteLine("UIManager: Trash Button Clicked.");
                            gameManager.PlayerTrashedDrink();
                        }
                    }
                    break;
            }
        }


        private void UpdatePauseMenuInput(GameManager gameManager)
        {
            Vector2 mousePosition = this.GameCursorPosition;
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                if (Raylib.CheckCollisionPointRec(mousePosition, _resumeButtonDest)) gameManager.ResumeGame();
                else if (Raylib.CheckCollisionPointRec(mousePosition, _pauseSettingsButtonDest)) gameManager.OpenSettings();
                else if (Raylib.CheckCollisionPointRec(mousePosition, _pauseMainMenuButtonDest)) gameManager.ReturnToMainMenu();
                else if (Raylib.CheckCollisionPointRec(mousePosition, _pauseExitGameButtonDest)) gameManager.RequestQuit();
            }
        }

        private void UpdateGameOverInput(GameManager gameManager)
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.Left) || Raylib.IsKeyPressed(KeyboardKey.Enter))
            {
                gameManager.ReturnToMainMenu();
            }
        }

        private void UpdateSettingsScreenInput(GameManager gameManager)
        {
            Vector2 mousePosition = this.GameCursorPosition;
            UpdateVolumeSliderInput(mousePosition);
            if (Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                if (Raylib.CheckCollisionPointRec(mousePosition, _settingsScreenBackButtonDest))
                {
                    gameManager.CloseSettings(); 
                }
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                gameManager.CloseSettings();
            }
        }

        private void UpdateVolumeSliderInput(Vector2 mousePosition)
        {
            bool isMouseOverKnob = Raylib.CheckCollisionPointRec(mousePosition, _volumeSliderKnobRect);

            if (Raylib.IsMouseButtonPressed(MouseButton.Left) && isMouseOverKnob)
            {
                _isDraggingVolumeKnob = true;
            }

            if (Raylib.IsMouseButtonDown(MouseButton.Left) && _isDraggingVolumeKnob)
            {
                float newKnobX = mousePosition.X;
                float clampedKnobX = Math.Clamp(newKnobX, _volumeSliderBarRect.X, _volumeSliderBarRect.X + _volumeSliderBarRect.Width);

                _masterVolume = (clampedKnobX - _volumeSliderBarRect.X) / _volumeSliderBarRect.Width;

                Raylib.SetMasterVolume(_masterVolume);
                Console.WriteLine($"Volume set to: {_masterVolume:P0}");

                _volumeSliderKnobRect.X = clampedKnobX - VOLUME_KNOB_RADIUS;
            }
            else if (Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                _isDraggingVolumeKnob = false;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Left))
            {
                _masterVolume = Math.Max(0.0f, _masterVolume - 0.05f);
                Raylib.SetMasterVolume(_masterVolume);
                _volumeSliderKnobRect.X = _volumeSliderBarRect.X + (_masterVolume * VOLUME_SLIDER_WIDTH) - VOLUME_KNOB_RADIUS;
                Console.WriteLine($"Volume set to: {_masterVolume:P0} (via Left Arrow)");
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Right))
            {
                _masterVolume = Math.Min(1.0f, _masterVolume + 0.05f);
                Raylib.SetMasterVolume(_masterVolume);
                _volumeSliderKnobRect.X = _volumeSliderBarRect.X + (_masterVolume * VOLUME_SLIDER_WIDTH) - VOLUME_KNOB_RADIUS;
                Console.WriteLine($"Volume set to: {_masterVolume:P0} (via Right Arrow)");
            }
        }
        

        public void DrawCurrentView(GameManager gameManager)
        {
            switch (gameManager.CurrentGameView)
            {
                case GameView.MainMenuScreen: DrawMainMenu(gameManager); break;
                case GameView.LevelIntroScreen: DrawLevelIntroScreen(gameManager); break;
                case GameView.CustomerScreen: DrawCustomerScreen(gameManager); break;
                case GameView.WorkstationScreen: DrawWorkstationScreen(gameManager); break;
                case GameView.DialogueScreen: DrawDialogueScreen(gameManager); break;
            };
        }

        public void DrawCursor()
        {
            Raylib.HideCursor();
            if (_customCursorTexture.Id != 0)
            {
                Raylib.DrawTextureV(_customCursorTexture, this.GameCursorPosition, Color.White);
            }
        }

        public void DrawGameCompleteScreen(GameManager gameManager)
        {
            if (_nightCoffeeBgTexture.Id != 0)
            {
                Raylib.DrawTexturePro(_nightCoffeeBgTexture, new Rectangle(0, 0, _nightCoffeeBgTexture.Width, _nightCoffeeBgTexture.Height), new Rectangle(0, 0, _screenWidth, _screenHeight), Vector2.Zero, 0f, Color.White);
            }
            else
            {
                Raylib.ClearBackground(new Color(20, 10, 30, 255)); 
            }

            Raylib.DrawRectangle(0, 0, _screenWidth, _screenHeight, new Color(0, 0, 0, 150));
            
            string title = "Congratulations, Master Barista!";
            string line1 = "You've successfully served the patrons of two worlds";
            string line2 = "and mastered the supernatural cafe!";
            string scoreText = $"Final Score: {gameManager.PlayerScore}";
            string prompt = "Ready for another shift?";

            float titleFontSize = 50;
            float lineFontSize = 24;
            float scoreFontSize = 32;
            float promptFontSize = 22;

            Vector2 titleSize = Raylib.MeasureTextEx(_mainFont, title, titleFontSize, 2);
            Vector2 line1Size = Raylib.MeasureTextEx(_mainFont, line1, lineFontSize, 1);
            Vector2 line2Size = Raylib.MeasureTextEx(_mainFont, line2, lineFontSize, 1);
            Vector2 scoreSize = Raylib.MeasureTextEx(_mainFont, scoreText, scoreFontSize, 2);
            Vector2 promptSize = Raylib.MeasureTextEx(_mainFont, prompt, promptFontSize, 1);

            Raylib.DrawTextEx(_mainFont, title, new Vector2((_screenWidth - titleSize.X) / 2, _screenHeight * 0.2f), titleFontSize, 2, Color.Gold);
            Raylib.DrawTextEx(_mainFont, line1, new Vector2((_screenWidth - line1Size.X) / 2, _screenHeight * 0.35f), lineFontSize, 1, Color.White);
            Raylib.DrawTextEx(_mainFont, line2, new Vector2((_screenWidth - line2Size.X) / 2, _screenHeight * 0.35f + 30), lineFontSize, 1, Color.White);
            Raylib.DrawTextEx(_mainFont, scoreText, new Vector2((_screenWidth - scoreSize.X) / 2, _screenHeight * 0.5f), scoreFontSize, 2, Color.SkyBlue);
            Raylib.DrawTextEx(_mainFont, prompt, new Vector2((_screenWidth - promptSize.X) / 2, _gameCompleteRestartButtonRect.Y - 40), promptFontSize, 1, Color.LightGray);
            
            bool mouseOverRestart = Raylib.CheckCollisionPointRec(this.GameCursorPosition, _gameCompleteRestartButtonRect);
            DrawTextButton("Play Again", _gameCompleteRestartButtonRect, mouseOverRestart, 20, Color.Black, new Color(120, 220, 120, 255), Color.Black, new Color(180, 255, 180, 255));

            bool mouseOverMenu = Raylib.CheckCollisionPointRec(this.GameCursorPosition, _gameCompleteMenuButtonRect);
            DrawTextButton("Main Menu", _gameCompleteMenuButtonRect, mouseOverMenu, 20, Color.Black, new Color(220, 120, 120, 255), Color.Black, new Color(255, 180, 180, 255));
        }

        private void DrawMainMenu(GameManager gameManager)
        {
            if (_menuBgSpriteSheetLoadedSuccessfully)
            {
                Rectangle sourceRec = new Rectangle(_menuBgCurrentFrame * _menuBgFrameWidth, 0, _menuBgFrameWidth, _menuBgFrameHeight);
                Rectangle destRec = new Rectangle(0, 0, _screenWidth, _screenHeight);
                Raylib.DrawTexturePro(_menuBackgroundTexture, sourceRec, destRec, Vector2.Zero, 0f, Color.White);
            }
            else if (_menuBackgroundTexture.Id != 0)
            {
                Raylib.DrawTexturePro(_menuBackgroundTexture, new Rectangle(0, 0, _menuBackgroundTexture.Width, _menuBackgroundTexture.Height), new Rectangle(0, 0, _screenWidth, _screenHeight), Vector2.Zero, 0f, Color.White);
            }
            else { Raylib.ClearBackground(Color.DarkGray); }

            string title = "Supernatural Coffee Shop";
            int titleFontSize = Math.Min(70, _screenWidth / 15);
            Vector2 titleSize = Raylib.MeasureTextEx(_mainFont, title, titleFontSize, 2);
            Raylib.DrawTextEx(_mainFont, title, new Vector2(_screenWidth / 2f - titleSize.X / 2f, _screenHeight * 0.25f), titleFontSize, 2, Color.White);

            DrawTextButton("Play Game", _playButtonDest, Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), _playButtonDest));
            DrawTextButton("Exit", _exitButtonDest, Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), _exitButtonDest));

            if (_settingsIconTexture.Id != 0)
            { Raylib.DrawTexturePro(_settingsIconTexture, new Rectangle(0, 0, _settingsIconTexture.Width, _settingsIconTexture.Height), _settingsIconDestRect, Vector2.Zero, 0f, Color.White); }
            else
            { Raylib.DrawRectangleRec(_settingsIconDestRect, Color.Gray); Raylib.DrawText("S", (int)_settingsIconDestRect.X + 15, (int)_settingsIconDestRect.Y + 10, 20, Color.Black); }
        }

        private void DrawTextButton(string text, Rectangle bounds, bool isMouseOver, int fontSize = 0,
                                    Color? nonHoverTextColor = null, Color? nonHoverBgColor = null,
                                    Color? hoverTextColor = null, Color? hoverBgColor = null)
        {
            Color defaultNonHoverBg = new Color(100, 100, 100, 220);
            Color defaultHoverBg = new Color(180, 180, 180, 255);
            Color defaultNonHoverText = Color.White;
            Color defaultHoverText = Color.Yellow;

            Color currentBgColor;
            Color currentTextColor;

            if (isMouseOver)
            {
                currentBgColor = hoverBgColor ?? defaultHoverBg;
                currentTextColor = hoverTextColor ?? defaultHoverText;
            }
            else
            {
                currentBgColor = nonHoverBgColor ?? defaultNonHoverBg;
                currentTextColor = nonHoverTextColor ?? defaultNonHoverText;
            }

            if (fontSize == 0) fontSize = Math.Max(18, (int)(bounds.Height * 0.45f));
            Raylib.DrawRectangleRounded(bounds, 0.2f, 4, currentBgColor);
            Vector2 textSize = Raylib.MeasureTextEx(_mainFont, text, fontSize, 1);
            Raylib.DrawTextEx(_mainFont, text, new Vector2(bounds.X + (bounds.Width - textSize.X) / 2f, bounds.Y + (bounds.Height - textSize.Y) / 2f), fontSize, 1, currentTextColor);
        }

        private void DrawLevelIntroScreen(GameManager gameManager)
        {
            if (_levelIntroScreenBgTexture.Id != 0)
            { Raylib.DrawTexturePro(_levelIntroScreenBgTexture, new Rectangle(0, 0, _levelIntroScreenBgTexture.Width, _levelIntroScreenBgTexture.Height), new Rectangle(0, 0, _screenWidth, _screenHeight), Vector2.Zero, 0f, Color.White); }
            else { Raylib.ClearBackground(Color.Black); }

            string levelText = "Level " + (gameManager.CurrentLevel?.LevelID?.Replace("Level_", "") ?? "N/A");
            string objectiveText = (gameManager.CurrentLevel != null) ? $"Target: ${gameManager.CurrentLevel.TargetEarnings:F0} in {gameManager.CurrentLevel.TimeLimit:F0}s" : "Loading...";

            int levelFontSize = 60; int objectiveFontSize = 30;
            Vector2 levelTextSize = Raylib.MeasureTextEx(_mainFont, levelText, levelFontSize, 2);
            Vector2 objectiveTextSize = Raylib.MeasureTextEx(_mainFont, objectiveText, objectiveFontSize, 2);

            Raylib.DrawTextEx(_mainFont, levelText, new Vector2(_screenWidth / 2f - levelTextSize.X / 2f + 2, _screenHeight / 2f - levelTextSize.Y / 2f - 20 + 2), levelFontSize, 2, Color.Black);
            Raylib.DrawTextEx(_mainFont, levelText, new Vector2(_screenWidth / 2f - levelTextSize.X / 2f, _screenHeight / 2f - levelTextSize.Y / 2f - 20), levelFontSize, 2, Color.White);
            Raylib.DrawTextEx(_mainFont, objectiveText, new Vector2(_screenWidth / 2f - objectiveTextSize.X / 2f + 1, _screenHeight / 2f + levelTextSize.Y / 2f + 10 + 1), objectiveFontSize, 2, Color.Black);
            Raylib.DrawTextEx(_mainFont, objectiveText, new Vector2(_screenWidth / 2f - objectiveTextSize.X / 2f, _screenHeight / 2f + levelTextSize.Y / 2f + 10), objectiveFontSize, 2, Color.LightGray);
        }

        private Rectangle CalculateCoveringDestRect(Rectangle sourceRect, float screenWidth, float screenHeight, float zoomFactor)
        {
            if (sourceRect.Width == 0 || sourceRect.Height == 0)
            {
                // Console.WriteLine("[CalculateCoveringDestRect] Warning: SourceRect has zero width or height."); 
                return new Rectangle(0, 0, screenWidth, screenHeight);
            }

            float frameWidth = sourceRect.Width;
            float frameHeight = sourceRect.Height;

            float frameAspectRatio = frameWidth / frameHeight;
            float screenAspectRatio = screenWidth / screenHeight;

            float baseDestWidth, baseDestHeight;
            if (frameAspectRatio > screenAspectRatio)
            {
                baseDestHeight = screenHeight;
                baseDestWidth = baseDestHeight * frameAspectRatio;
            }
            else
            {
                baseDestWidth = screenWidth;
                baseDestHeight = baseDestWidth / frameAspectRatio;
            }

            float zoomedDestWidth = baseDestWidth * zoomFactor;
            float zoomedDestHeight = baseDestHeight * zoomFactor;
            float destX = (screenWidth - zoomedDestWidth) / 2f;
            float destY = (screenHeight - zoomedDestHeight) / 2f;

            return new Rectangle(destX, destY, zoomedDestWidth, zoomedDestHeight);
        }

        private void DrawCustomerScreen(GameManager gameManager)
        {
            //Console.WriteLine($"[UIManager.DrawCustomerScreen :: ENTRY] GhostInterferenceActive = {gameManager.IsGhostInterferenceActive}, CurrentView = {gameManager.CurrentGameView}, InteractingCust: {gameManager.CurrentlyInteractingCustomer?.CustomerID ?? "None"}");

            _customerClickAreas.Clear();

            // 1. Draw Background
            if (_customerScreenBgTexture.Id != 0)
            {
                Raylib.DrawTexturePro(_customerScreenBgTexture, new Rectangle(0, 0, _customerScreenBgTexture.Width, _customerScreenBgTexture.Height), new Rectangle(0, 0, _screenWidth, _screenHeight), Vector2.Zero, 0f, Color.White);
            }
            else
            {
                Raylib.ClearBackground(Color.SkyBlue);
            }

            // 2. Draw Heatwave Effect (if FireMonster is present)
            // Checks if FireMonster is in queue OR currently interacting
            bool fireMonsterPresent = gameManager.ShopLayout.CustomerQueue.Any(cust => cust is SupernaturalCustomer sc && sc.Type == SupernaturalCustomerType.FireMonster) ||
                                    (gameManager.CurrentlyInteractingCustomer is SupernaturalCustomer sciFm && sciFm.Type == SupernaturalCustomerType.FireMonster);

            if (_heatwaveEffectTexture.Id != 0 && fireMonsterPresent)
            {
                if (_heatwaveFrames != null && _currentHeatwaveFrame >= 0 && _currentHeatwaveFrame < _heatwaveFrames.Length && _heatwaveFrames.Length > 0)
                {
                    Rectangle sourceRect = _heatwaveFrames[_currentHeatwaveFrame];
                    Rectangle destRect = CalculateCoveringDestRect(sourceRect, _screenWidth, _screenHeight, _heatwaveZoomFactor);
                    Raylib.DrawTexturePro(_heatwaveEffectTexture, sourceRect, destRect, Vector2.Zero, 0f, new Color(255, 220, 200, 70));
                }
            }

            // 3. GHOST INTERFERENCE EFFECTS (Overlay + Dimming)
            if (gameManager.IsGhostInterferenceActive)
            {
                //Console.WriteLine($"[UIManager.DrawCustomerScreen] INSIDE GhostInterferenceActive block. GhostTexID: {_ghostScreenEffectTexture.Id}, GhostCurrentFrame: {_currentGhostScreenEffectFrame}");

                // 3a. Draw Ghost Spritesheet Overlay
                if (_ghostScreenEffectTexture.Id != 0 &&
                    _ghostScreenEffectFrames != null &&
                    _ghostScreenEffectFrames.Length > 0 && // Check length before indexing
                    _currentGhostScreenEffectFrame >= 0 &&
                    _currentGhostScreenEffectFrame < _ghostScreenEffectFrames.Length)
                {
                    //Console.WriteLine($"[UIManager.DrawCustomerScreen] DRAWING GHOST SPRITESHEET frame {_currentGhostScreenEffectFrame}. Source W:{_ghostScreenEffectFrames[_currentGhostScreenEffectFrame].Width} H:{_ghostScreenEffectFrames[_currentGhostScreenEffectFrame].Height}");
                    Rectangle ghostSourceRect = _ghostScreenEffectFrames[_currentGhostScreenEffectFrame];
                    Rectangle ghostDestRect = CalculateCoveringDestRect(ghostSourceRect, _screenWidth, _screenHeight, _ghostEffectZoomFactor);

                    Color desiredGhostOverlayTint = new Color(200, 220, 255, 100);
                    Raylib.DrawTexturePro(_ghostScreenEffectTexture, ghostSourceRect, ghostDestRect, Vector2.Zero, 0f, desiredGhostOverlayTint);
                }
                else
                {
                    //Console.WriteLine($"[UIManager.DrawCustomerScreen] Ghost spritesheet overlay SKIPPED: TexID={_ghostScreenEffectTexture.Id}, FramesNullOrEmpty={_ghostScreenEffectFrames == null || _ghostScreenEffectFrames.Length == 0}, InterferenceActive={gameManager.IsGhostInterferenceActive}, CurrentFrame={_currentGhostScreenEffectFrame}, FramesLength={_ghostScreenEffectFrames?.Length ?? -1}");
                }
            }

            if (gameManager.IsToyolHiding && gameManager.CurrentGameView == GameView.CustomerScreen)
            {
                if (_hiddenToyolIconTexture.Id != 0)
                {
                    Rectangle toyolSourceFrame = new Rectangle(0, 0, _hiddenToyolIconTexture.Width, _hiddenToyolIconTexture.Height);

                    Rectangle destRectForHiddenToyol = gameManager.HiddenToyolClickRect;

                    //Console.WriteLine($"[UIManager.DrawCustomerScreen] Drawing HIDDEN Toyol using _hiddenToyolIconTexture (ID: {_hiddenToyolIconTexture.Id}) at {destRectForHiddenToyol.X:F0},{destRectForHiddenToyol.Y:F0} Size: {destRectForHiddenToyol.Width:F0}x{destRectForHiddenToyol.Height:F0}");

                    Raylib.DrawTexturePro(
                        _hiddenToyolIconTexture,
                        toyolSourceFrame,
                        destRectForHiddenToyol,
                        Vector2.Zero,
                        0f,
                        Color.White
                    );
                }
                else
                {
                    Console.WriteLine($"[UIManager.DrawCustomerScreen] Hidden Toyol icon texture not loaded. Drawing fallback rectangle.");
                    Raylib.DrawRectangleRec(gameManager.HiddenToyolClickRect, new Color(70, 0, 70, 150));
                }
            }

            // 4. Draw Customers in Queue 
            if (gameManager.ShopLayout?.CustomerQueue != null)
            {
                Customer[] customersInQueue = gameManager.ShopLayout.CustomerQueue.ToArray();
                Vector2[] queuePositions = { _customerQueueSlot1Pos, _customerQueueSlot2Pos, _customerQueueSlot3Pos };

                _customerClickAreas.Clear();

                for (int i = 0; i < customersInQueue.Length && i < queuePositions.Length; i++)
                {
                    Customer cust = customersInQueue[i];
                    if (cust == null) continue;

                    Texture2D custInQueueTex = cust.GetInQueueTexture();
                    Rectangle custSourceRec;
                    Rectangle custDestRec;

                    if (custInQueueTex.Id != 0 && custInQueueTex.Width > 0 && custInQueueTex.Height > 0)
                    {
                        custSourceRec = cust.GetCurrentDialogueFrameSourceRect();

                        if (custSourceRec.Width <= 0 || custSourceRec.Height <= 0 ||
                            custSourceRec.X < 0 || custSourceRec.Y < 0 ||
                            custSourceRec.X + custSourceRec.Width > custInQueueTex.Width ||
                            custSourceRec.Y + custSourceRec.Height > custInQueueTex.Height)
                        {
                            custSourceRec = new Rectangle(0, 0, custInQueueTex.Width, custInQueueTex.Height);
                        }

                        custDestRec = new Rectangle(
                            queuePositions[i].X,
                            queuePositions[i].Y,
                            custSourceRec.Width * _customerDrawScale,
                            custSourceRec.Height * _customerDrawScale
                        );
                        Raylib.DrawTexturePro(custInQueueTex, custSourceRec, custDestRec, Vector2.Zero, 0f, Color.White);
                    }
                    else
                    {
                        float fallbackWidth = 50 * _customerDrawScale;
                        float fallbackHeight = 100 * _customerDrawScale;
                        custDestRec = new Rectangle(
                            queuePositions[i].X,
                            queuePositions[i].Y,
                            fallbackWidth,
                            fallbackHeight
                        );
                        Raylib.DrawRectangleRec(custDestRec, Color.LightGray);
                    }

                    // --- Calculate dimensions for state text and patience meter ---
                    string stateText = $"({cust.CurrentState})";
                    Vector2 stateTextSize = Raylib.MeasureTextEx(_mainFont, stateText, 10, 1);

                    float meterWidth = 0;
                    float meterHeight = 0;
                    Rectangle patienceMeterSourceToDraw = new Rectangle();
                    int patienceFrameIndex = 0;

                    if (_patienceMeterTexture.Id != 0 && _patienceMeterFrames != null && _patienceMeterFrames.Length == MAX_PATIENCE_FRAMES)
                    {
                        float patiencePercent = cust.Patience / Customer.INITIAL_PATIENCE;
                        patiencePercent = Math.Clamp(patiencePercent, 0.0f, 1.0f);

                        patienceFrameIndex = (MAX_PATIENCE_FRAMES - 1) - (int)Math.Round(patiencePercent * (MAX_PATIENCE_FRAMES - 1));
                        patienceFrameIndex = Math.Clamp(patienceFrameIndex, 0, MAX_PATIENCE_FRAMES - 1);

                        if (patienceFrameIndex < _patienceMeterFrames.Length)
                        {
                            patienceMeterSourceToDraw = _patienceMeterFrames[patienceFrameIndex];
                            meterWidth = patienceMeterSourceToDraw.Width * _queuePatienceMeterScale;
                            meterHeight = patienceMeterSourceToDraw.Height * _queuePatienceMeterScale;
                        }
                    }

                    float paddingBetweenMeterAndText = 2f;
                    float paddingBetweenTextAndCustomerSpriteTop = 5f;

                    float textActualY = custDestRec.Y - paddingBetweenTextAndCustomerSpriteTop - stateTextSize.Y;
                    float meterActualY = textActualY - paddingBetweenMeterAndText - meterHeight;

                    float commonCenterX = custDestRec.X + (custDestRec.Width / 2f);

                    // --- Draw Patience Meter ---
                    if (_patienceMeterTexture.Id != 0 && meterHeight > 0 && patienceMeterSourceToDraw.Width > 0)
                    {
                        Vector2 patienceMeterDrawPos = new Vector2(
                            commonCenterX - (meterWidth / 2f),
                            meterActualY
                        );
                        Raylib.DrawTexturePro(_patienceMeterTexture, patienceMeterSourceToDraw,
                                            new Rectangle(patienceMeterDrawPos.X, patienceMeterDrawPos.Y, meterWidth, meterHeight),
                                            Vector2.Zero, 0f, Color.White);
                    }

                    // --- Draw State Text ---
                    Raylib.DrawTextEx(_mainFont, stateText,
                                    new Vector2(commonCenterX - (stateTextSize.X / 2f), textActualY),
                                    10, 1, Color.White);

                    _customerClickAreas.Add(new Tuple<Customer, Rectangle>(cust, custDestRec));
                }
            }

            // 5. Draw HUD 
            DrawInGameHud(gameManager);

            // 6. Draw Customer Screen Trash Can
            if (gameManager.Player.HeldItem is Drink)
            {
                if (_trashButtonTexture.Id != 0)
                {
                    bool mouseOverTrash = Raylib.CheckCollisionPointRec(GameCursorPosition, _customerScreenTrashCanRect);
                    Raylib.DrawTexturePro(
                        _trashButtonTexture,
                        new Rectangle(0, 0, _trashButtonTexture.Width, _trashButtonTexture.Height),
                        _customerScreenTrashCanRect,
                        Vector2.Zero,
                        0f,
                        mouseOverTrash ? Color.LightGray : Color.White
                    );
                    if (mouseOverTrash)
                    {
                        Raylib.DrawTextEx(_mainFont, "Discard Held Drink", new Vector2(_customerScreenTrashCanRect.X - 100, _customerScreenTrashCanRect.Y - 5), 18, 1, Color.White);
                    }
                }
                else
                {
                    Raylib.DrawRectangleRec(_customerScreenTrashCanRect, Color.Red);
                    Raylib.DrawTextEx(_mainFont, "T", new Vector2(_customerScreenTrashCanRect.X + 15, _customerScreenTrashCanRect.Y + 10), 30, 1, Color.White);
                }
                //Console.WriteLine("[UIManager.DrawCustomerScreen] Trash can drawn because player is holding a drink.");
            }
            else
            {
                // Console.WriteLine("[UIManager.DrawCustomerScreen] Trash can NOT drawn (player not holding drink).");
            }

            if (_gameManagerInstance!.IsToyolHiding && _gameManagerInstance.CurrentGameView == GameView.CustomerScreen)
            {
                Customer? anyToyolInQueue = _gameManagerInstance.ShopLayout?.CustomerQueue.FirstOrDefault(c => c is SupernaturalCustomer sc && sc.Type == SupernaturalCustomerType.Toyol);
                Texture2D toyolSpriteToDraw = new Texture2D { Id = 0 };
                Rectangle toyolSourceFrame = new Rectangle(0, 0, 1, 1);

                if (_hiddenToyolIconTexture.Id != 0)
                {
                    toyolSpriteToDraw = _hiddenToyolIconTexture;
                    toyolSourceFrame = new Rectangle(0, 0, _hiddenToyolIconTexture.Width, _hiddenToyolIconTexture.Height);
                }
                else if (anyToyolInQueue != null)
                {
                    toyolSpriteToDraw = anyToyolInQueue.GetDialogueSheetTexture();
                    Rectangle[]? frames = (anyToyolInQueue as SupernaturalCustomer)?.GetDialogueSheetFrames();
                    if (frames != null && frames.Length > 0)
                    {
                        toyolSourceFrame = frames[0];
                    }
                    else if (toyolSpriteToDraw.Id != 0)
                    {
                        toyolSourceFrame = new Rectangle(0, 0, toyolSpriteToDraw.Width, toyolSpriteToDraw.Height);
                    }
                }

                if (toyolSpriteToDraw.Id != 0 && toyolSourceFrame.Width > 1)
                {
                    if (gameManager != null)
                    {
                        //Console.WriteLine($"[UIManager.DrawCustomerScreen] Drawing HIDDEN Toyol at {gameManager.HiddenToyolClickRect.X},{gameManager.HiddenToyolClickRect.Y}");
                        Raylib.DrawTexturePro(
                            toyolSpriteToDraw,
                            toyolSourceFrame,
                            gameManager.HiddenToyolClickRect,
                            Vector2.Zero,
                            0f,
                            Color.White
                        );
                    }
                    else
                    {
                        Console.WriteLine("[UIManager.DrawCustomerScreen] ERROR: gameManager parameter is null when trying to draw Toyol's click rectangle.");
                    }
                }
                else
                {
                    Rectangle fallbackRect = (gameManager != null) ? gameManager.HiddenToyolClickRect : _gameManagerInstance.HiddenToyolClickRect;
                    Console.WriteLine($"[UIManager.DrawCustomerScreen] Hidden Toyol icon texture not loaded or source frame invalid. Drawing fallback rectangle.");
                    Raylib.DrawRectangleRec(fallbackRect, new Color(70, 0, 70, 150));
                }
            }

            // 7. Draw Pause Button 
            if (gameManager?.GameState == GameState.Playing)
            {
                if (_pauseButtonTexture.Id != 0)
                { Raylib.DrawTexturePro(_pauseButtonTexture, new Rectangle(0, 0, _pauseButtonTexture.Width, _pauseButtonTexture.Height), _pauseButtonScreenRect, Vector2.Zero, 0f, Color.White); }
                else
                { Raylib.DrawRectangleRec(_pauseButtonScreenRect, Color.DarkGray); Raylib.DrawTextEx(_mainFont, "P", new Vector2(_pauseButtonScreenRect.X + _pauseButtonScreenRect.Width / 2 - Raylib.MeasureTextEx(_mainFont, "P", 20, 1).X / 2, _pauseButtonScreenRect.Y + _pauseButtonScreenRect.Height / 2 - 10), 20, 1, Color.White); }
            }

        }

        private void DrawInGameHud(GameManager gameManager)
        {
            // 1. Standard Text HUD Elements (Top-Left)
            float currentTextHudY = 20f;
            float hudX = 20f;
            float lineSpacing = 25f;

            Raylib.DrawTextEx(_mainFont, $"Score: {gameManager.PlayerScore}", new Vector2(hudX, currentTextHudY), 20, 1, Color.White);
            currentTextHudY += lineSpacing;
            Raylib.DrawTextEx(_mainFont, $"Time: {(int)gameManager.GameTime}", new Vector2(hudX, currentTextHudY), 20, 1, Color.White);
            currentTextHudY += lineSpacing;
            Raylib.DrawTextEx(_mainFont, $"Money: ${gameManager.Player.Money:F2}", new Vector2(hudX, currentTextHudY), 20, 1, Color.White);
            currentTextHudY += lineSpacing;
            Raylib.DrawTextEx(_mainFont, $"Water Jugs: {gameManager.Player.WaterJugCount}", new Vector2(hudX, currentTextHudY), 20, 1, Color.SkyBlue);


            // 2. Level Information (Top-Center)
            float nextCentralY = 20f;

            if (gameManager.CurrentLevel != null)
            {
                string levelInfo = $"Level: {gameManager.CurrentLevel.LevelID.Replace("Level_", "")} | Target: ${gameManager.CurrentLevel.TargetEarnings:F0}";
                Vector2 levelInfoSize = Raylib.MeasureTextEx(_mainFont, levelInfo, 20, 1);
                Raylib.DrawTextEx(_mainFont, levelInfo, new Vector2((_screenWidth - levelInfoSize.X) / 2f, nextCentralY), 20, 1, Color.Yellow);
                nextCentralY += levelInfoSize.Y + 10;
            }
            else
            {
                nextCentralY += 20f + 10f;
            }

            // 3. Graphical Sanity Meter (Positioned below Level Info)
            if (_sanityMeterTexture.Id != 0 && _sanityMeterBorderSourceRect.Width > 0)
            {
                Vector2 sanityMeterActualScreenPos = new Vector2(
                    (_screenWidth - _sanityMeterBorderSourceRect.Width) / 2f,
                    nextCentralY
                );

                float sanity = gameManager.Player.SanityLevel;
                int totalSegments = 5;
                int visibleSegments;
                if (sanity > 80) visibleSegments = 5;
                else if (sanity > 60) visibleSegments = 4;
                else if (sanity > 40) visibleSegments = 3;
                else if (sanity > 20) visibleSegments = 2;
                else if (sanity > 0) visibleSegments = 1;
                else visibleSegments = 0;
                visibleSegments = Math.Clamp(visibleSegments, 0, totalSegments);

                float segmentWidthSource = _sanityMeterFillSourceBaseRect.Width / totalSegments;
                float currentFillWidthSource = segmentWidthSource * visibleSegments;

                if (currentFillWidthSource > 0)
                {
                    Rectangle currentSanityFillSourceRect = new Rectangle(
                        _sanityMeterFillSourceBaseRect.X,
                        _sanityMeterFillSourceBaseRect.Y,
                        currentFillWidthSource,
                        _sanityMeterFillSourceBaseRect.Height
                    );
                    Vector2 fillScreenPos = new Vector2(
                        sanityMeterActualScreenPos.X + (_sanityMeterFillSourceBaseRect.X - _sanityMeterBorderSourceRect.X),
                        sanityMeterActualScreenPos.Y + (_sanityMeterFillSourceBaseRect.Y - _sanityMeterBorderSourceRect.Y) - (float)38.5
                    );
                    Raylib.DrawTextureRec(_sanityMeterTexture, currentSanityFillSourceRect, fillScreenPos, Color.White);
                }

                Raylib.DrawTextureRec(_sanityMeterTexture, _sanityMeterBorderSourceRect, sanityMeterActualScreenPos, Color.White);
            }

            // 4. Low Stock Warning (Bottom Right area)
            _isLowStockWarningActive = false;
            if (gameManager.Player.Inventory != null)
            {
                foreach (var itemEntry in gameManager.Player.Inventory)
                {
                    if (itemEntry.Value <= LOW_STOCK_THRESHOLD) { _isLowStockWarningActive = true; break; }
                }
            }
            if (_isLowStockWarningActive)
            {
                string warningText = "INGREDIENT STOCK LOW!";
                Vector2 warningSize = Raylib.MeasureTextEx(_mainFont, warningText, 20, 1);
                Raylib.DrawTextEx(_mainFont, warningText, new Vector2(_screenWidth - warningSize.X - 20, _screenHeight - 110), 20, 1, Color.Orange);
            }

            // 5. Shop Button 
            if (gameManager.CurrentGameView == GameView.CustomerScreen && gameManager.GameState == GameState.Playing)
            {
                DrawTextButton("Shop", _shopAccessButtonDest, Raylib.CheckCollisionPointRec(this.GameCursorPosition, _shopAccessButtonDest), 18);
            }
        }

        public void DrawPauseMenu(GameManager gameManager)
        {
            Raylib.DrawRectangle(0, 0, _screenWidth, _screenHeight, new Color(0, 0, 0, 180));
            Raylib.DrawTextEx(_mainFont, "PAUSED", new Vector2(_screenWidth / 2f - Raylib.MeasureTextEx(_mainFont, "PAUSED", 50, 2).X / 2f, _screenHeight * 0.2f), 50, 2, Color.Yellow);
            DrawTextButton("Resume", _resumeButtonDest, Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), _resumeButtonDest));
            DrawTextButton("Settings", _pauseSettingsButtonDest, Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), _pauseSettingsButtonDest));
            DrawTextButton("Return to Main Menu", _pauseMainMenuButtonDest, Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), _pauseMainMenuButtonDest));
            DrawTextButton("Exit Game", _pauseExitGameButtonDest, Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), _pauseExitGameButtonDest));
        }

        public void DrawSettingsScreen(GameManager gameManager)
        {
            if (_settingsScreenBgTexture.Id != 0)
            { Raylib.DrawTexturePro(_settingsScreenBgTexture, new Rectangle(0, 0, _settingsScreenBgTexture.Width, _settingsScreenBgTexture.Height), new Rectangle(0, 0, _screenWidth, _screenHeight), Vector2.Zero, 0f, Color.White); }
            else { Raylib.ClearBackground(Color.DarkBlue); }

            // Draw the "SETTINGS" title.
            string settingsTitle = "SETTINGS";
            int titleFontSize = 40;
            Vector2 titleTextSize = Raylib.MeasureTextEx(_mainFont, settingsTitle, titleFontSize, 2);
            Raylib.DrawTextEx(
                _mainFont,
                settingsTitle,
                new Vector2(_screenWidth / 2f - titleTextSize.X / 2f, 50),
                titleFontSize,
                2,
                Color.White
            );

            DrawVolumeSlider();

            // Draw the "Back" button.
            bool isMouseOverBackButton = Raylib.CheckCollisionPointRec(this.GameCursorPosition, _settingsScreenBackButtonDest);
            DrawTextButton(
                "Back",
                _settingsScreenBackButtonDest,
                isMouseOverBackButton,
                20
            );
        }

        private void DrawVolumeSlider()
        {
            // Draw "Volume" label
            string volumeLabel = "Volume";
            int labelFontSize = 25;
            Vector2 labelTextSize = Raylib.MeasureTextEx(_mainFont, volumeLabel, labelFontSize, 1);
            Raylib.DrawTextEx(
                _mainFont,
                volumeLabel,
                new Vector2(_volumeSliderBarRect.X - labelTextSize.X - 15, _volumeSliderBarRect.Y + (_volumeSliderBarRect.Height / 2f) - (labelTextSize.Y / 2f)),
                labelFontSize,
                1,
                Color.White
            );

            // Draw slider bar
            Raylib.DrawRectangleRec(_volumeSliderBarRect, Color.DarkGray);

            // Draw current volume fill
            float fillWidth = _masterVolume * _volumeSliderBarRect.Width;
            Raylib.DrawRectangleRec(new Rectangle(_volumeSliderBarRect.X, _volumeSliderBarRect.Y, fillWidth, _volumeSliderBarRect.Height), Color.Green);

            // Draw slider knob
            Raylib.DrawCircleV(
                new Vector2(_volumeSliderKnobRect.X + VOLUME_KNOB_RADIUS, _volumeSliderKnobRect.Y + VOLUME_KNOB_RADIUS),
                VOLUME_KNOB_RADIUS,
                _isDraggingVolumeKnob || Raylib.CheckCollisionPointRec(this.GameCursorPosition, _volumeSliderKnobRect) ? Color.Green : Color.DarkGreen // Adjusted this line
            );

            // Draw volume percentage text
            string volumePercentText = $"{(_masterVolume * 100):F0}%";
            int percentFontSize = 20;
            Vector2 percentTextSize = Raylib.MeasureTextEx(_mainFont, volumePercentText, percentFontSize, 1);
            Raylib.DrawTextEx(
                _mainFont,
                volumePercentText,
                new Vector2(_volumeSliderBarRect.X + _volumeSliderBarRect.Width + 15, _volumeSliderBarRect.Y + (_volumeSliderBarRect.Height / 2f) - (percentTextSize.Y / 2f)),
                percentFontSize,
                1,
                Color.White
            );
        }

        private void DrawDialogueScreen(GameManager gameManager)
        {
            //Console.WriteLine($"[UIManager.DrawDialogueScreen :: ENTRY] GhostInterferenceActive = {gameManager.IsGhostInterferenceActive}, CurrentView = {gameManager.CurrentGameView}");

            // 1. Draw Background
            if (_dialogueBgTexture.Id != 0)
            {
                Raylib.DrawTexturePro(_dialogueBgTexture, new Rectangle(0, 0, _dialogueBgTexture.Width, _dialogueBgTexture.Height), new Rectangle(0, 0, _screenWidth, _screenHeight), Vector2.Zero, 0f, Color.White);
            }
            else { Raylib.ClearBackground(Color.LightGray); }

            // 2. Draw Heatwave Effect 
            bool shouldShowHeatwaveBasedOnInteractingCustomer = (gameManager.CurrentlyInteractingCustomer is SupernaturalCustomer sc_fire && sc_fire.Type == SupernaturalCustomerType.FireMonster);
            if (_heatwaveEffectTexture.Id != 0 && shouldShowHeatwaveBasedOnInteractingCustomer)
            {
                if (_heatwaveFrames != null && _currentHeatwaveFrame >= 0 && _currentHeatwaveFrame < _heatwaveFrames.Length && _heatwaveFrames.Length > 0)
                {
                    Rectangle sourceRect = _heatwaveFrames[_currentHeatwaveFrame];
                    Rectangle destRect = CalculateCoveringDestRect(sourceRect, _screenWidth, _screenHeight, _heatwaveZoomFactor);
                    // Console.WriteLine($"[UIManager.DrawDialogueScreen] DRAWING HEATWAVE frame {_currentHeatwaveFrame}.");
                    Raylib.DrawTexturePro(_heatwaveEffectTexture, sourceRect, destRect, Vector2.Zero, 0f, new Color(255, 220, 200, 60));
                }
            }

            // --- 3. GHOST INTERFERENCE EFFECTS DRAWING ---
            if (gameManager.IsGhostInterferenceActive)
            {
                //Console.WriteLine($"[UIManager.DrawDialogueScreen] INSIDE IsGhostInterferenceActive block.");

                //Console.WriteLine($"[UIManager.DrawDialogueScreen] INSIDE GhostInterferenceActive block. GhostTexID: {_ghostScreenEffectTexture.Id}, GhostCurrentFrame: {_currentGhostScreenEffectFrame}");

                // 3a. Draw Ghost Spritesheet Overlay
                if (_ghostScreenEffectTexture.Id != 0 &&
                    _ghostScreenEffectFrames != null &&
                    _currentGhostScreenEffectFrame >= 0 &&
                    _currentGhostScreenEffectFrame < _ghostScreenEffectFrames.Length &&
                    _ghostScreenEffectFrames.Length > 0)
                {
                    //Console.WriteLine($"[UIManager.DrawDialogueScreen] DRAWING GHOST SPRITESHEET frame {_currentGhostScreenEffectFrame}. Source W:{_ghostScreenEffectFrames[_currentGhostScreenEffectFrame].Width} H:{_ghostScreenEffectFrames[_currentGhostScreenEffectFrame].Height}");
                    Rectangle ghostSourceRect = _ghostScreenEffectFrames[_currentGhostScreenEffectFrame];
                    Rectangle ghostDestRect = CalculateCoveringDestRect(ghostSourceRect, _screenWidth, _screenHeight, _ghostEffectZoomFactor);

                    Color ghostOverlayTint = new Color(200, 220, 255, 100);
                    Raylib.DrawTexturePro(_ghostScreenEffectTexture, ghostSourceRect, ghostDestRect, Vector2.Zero, 0f, ghostOverlayTint);
                }
                else
                {
                    //Console.WriteLine($"[UIManager.DrawDialogueScreen] Ghost spritesheet overlay SKIPPED: TexID={_ghostScreenEffectTexture.Id}, FramesNullOrEmpty={_ghostScreenEffectFrames == null || _ghostScreenEffectFrames.Length == 0}, InterferenceActive={gameManager.IsGhostInterferenceActive}, CurrentFrame={_currentGhostScreenEffectFrame}, FramesLength={_ghostScreenEffectFrames?.Length ?? -1}");
                }

                // 3b. Draw Screen Dimming Overlay
                if (gameManager.ScreenDimAlpha > 0)
                {
                    //Console.WriteLine($"[UIManager.DrawDialogueScreen] Drawing screen dim. TargetAlpha: {gameManager.ScreenDimAlpha}");
                    Raylib.DrawRectangle(0, 0, _screenWidth, _screenHeight, new Color(0, 0, 0, (int)(gameManager.ScreenDimAlpha * 255)));
                }

            }

            // 4. Draw In-Game HUD (should be on top of general screen effects, but dialogue box/portrait might cover parts of it)
            DrawInGameHud(gameManager);

            // 5. Draw Dialogue Box, Text, and Character Portrait (these should be topmost for dialogue screen)
            Customer? interactingCustomer = gameManager.CurrentlyInteractingCustomer;
            if (interactingCustomer != null)
            {
                Dialogue dialogueSet = interactingCustomer.DialogueSet;
                string? speaker = dialogueSet.GetCurrentSpeaker();
                string? text = dialogueSet.GetCurrentText();

                if (dialogueSet.CurrentLineIndex == -1 && dialogueSet.Speakers.Count > 0)
                {
                    speaker = dialogueSet.Speakers[0];
                    text = dialogueSet.Texts[0];
                }
                else if (dialogueSet.CurrentLineIndex == -1)
                {
                    text = "[Press Enter or Click to hear the request...]";
                }

                if (speaker == null && text == null && dialogueSet.CurrentLineIndex == -1 && dialogueSet.HasMoreLines())
                {
                    text = "[Press Enter or Click to Start Dialogue]";
                }

                Texture2D dialogueSheet = interactingCustomer.GetDialogueSheetTexture();
                Rectangle dialogueFrameRect = interactingCustomer.GetCurrentDialogueFrameSourceRect();
                float dialoguePortraitScale = 1.0f;

                if (dialogueSheet.Id != 0 && dialogueFrameRect.Width > 1 && dialogueFrameRect.Height > 1)
                {
                    float portraitRenderWidth = dialogueFrameRect.Width * dialoguePortraitScale;
                    float portraitRenderHeight = dialogueFrameRect.Height * dialoguePortraitScale;

                    float portraitPadding = 20f;
                    float dialogueBoxHeight = 150;
                    float dialogueBoxBottomMargin = 30f;
                    float dialogueBoxTopY = _screenHeight - dialogueBoxHeight - dialogueBoxBottomMargin;

                    Vector2 portraitPos = new Vector2(
                        _screenWidth - portraitRenderWidth - portraitPadding,
                        dialogueBoxTopY - portraitRenderHeight - portraitPadding
                    );
                    if (portraitPos.Y < 70) portraitPos.Y = 70;

                    Raylib.DrawTexturePro(dialogueSheet, dialogueFrameRect,
                                        new Rectangle(portraitPos.X, portraitPos.Y, portraitRenderWidth, portraitRenderHeight),
                                        Vector2.Zero, 0f, Color.White);
                }

                float boxHeight = 150;
                float bottomMargin = 30f;
                float boxY = _screenHeight - boxHeight - bottomMargin;
                Rectangle dialogueBoxRect = new Rectangle(50, boxY, _screenWidth - 100, boxHeight);
                Raylib.DrawRectangleRec(dialogueBoxRect, new Color(0, 0, 0, 200));
                Raylib.DrawRectangleLinesEx(dialogueBoxRect, 2, Color.White);

                if (speaker != null)
                { Raylib.DrawTextEx(_mainFont, speaker + ":", new Vector2(dialogueBoxRect.X + 15, dialogueBoxRect.Y + 15), 20, 1, Color.Yellow); }

                if (text != null)
                {
                    DrawWrappedText(text,
                        new Rectangle(dialogueBoxRect.X + 15, dialogueBoxRect.Y + (speaker != null ? 45 : 25), dialogueBoxRect.Width - 30, dialogueBoxRect.Height - (speaker != null ? 60 : 40) - 15),
                        20, Color.White);
                }
                Raylib.DrawTextEx(_mainFont, "Click or Press Enter to Continue...",
                    new Vector2(dialogueBoxRect.X + 15, dialogueBoxRect.Y + dialogueBoxRect.Height - 25),
                    10, 1, Color.LightGray);
            }
            else
            {
                Raylib.DrawTextEx(_mainFont, "NO ACTIVE DIALOGUE CUSTOMER", new Vector2(20, _screenHeight - 100), 20, 1, Color.Orange);
            }
        }

        private void DrawWrappedText(string text, Rectangle bounds, float fontSize, Color color)
        {
            float currentX = bounds.X;
            float currentY = bounds.Y;
            float spaceWidth = Raylib.MeasureTextEx(_mainFont, " ", fontSize, 1).X;

            string[] words = text.Split(' ');

            foreach (string word in words)
            {
                if (string.IsNullOrWhiteSpace(word)) continue;

                Vector2 wordSize = Raylib.MeasureTextEx(_mainFont, word, fontSize, 1);

                if (currentX + wordSize.X > bounds.X + bounds.Width)
                {
                    currentY += fontSize + 2;
                    currentX = bounds.X;
                }

                if (currentY > bounds.Y + bounds.Height - fontSize)
                {
                    if (currentX < bounds.X + bounds.Width - Raylib.MeasureTextEx(_mainFont, "...", fontSize, 1).X)
                        Raylib.DrawTextEx(_mainFont, "...", new Vector2(currentX, currentY), fontSize, 1, color);
                    break;
                }

                Raylib.DrawTextEx(_mainFont, word, new Vector2(currentX, currentY), fontSize, 1, color);
                currentX += wordSize.X + spaceWidth;
            }
        }

        private void DrawWorkstationScreen(GameManager gameManager)
        {
            Rectangle actualTemplateDest = _wsTemplateDestRect;
            Rectangle centeredServeArea = new Rectangle();
            InteractionContext machineContext = _currentWorkstationMachineContext;

            bool drawShelf = false;
            bool drawTemplateAndItsContent = false;
            bool drawAnimationSlotsAndContent = false;
            bool drawCenteredServeUIWithCt2Bg = false;

            if (_workstationSpritesheet1.Id != 0 && _wsBackgroundSourceRect.Width > 0)
            {
                Raylib.DrawTexturePro(
                    _workstationSpritesheet1, _wsBackgroundSourceRect,
                    new Rectangle(0, 0, _screenWidth, _screenHeight),
                    Vector2.Zero, 0f, Color.White);
            }
            else
            {
                Raylib.ClearBackground(Color.DarkGray);
            }

            if (!gameManager.IsBrewingStationOverheated && machineContext.AvailableActions.Contains("Refill Water"))
            {
                bool mouseOverRefill = Raylib.CheckCollisionPointRec(this.GameCursorPosition, _refillWaterButtonRect);

                Color baseButtonColor = new Color(80, 120, 200, 255);
                Color hoverButtonColor = new Color(100, 150, 230, 255);
                Color textColor = Color.White;
                Color hoverTextColor = Color.LightGray;

                DrawTextButton("Refill Water", _refillWaterButtonRect, mouseOverRefill, 18,
                            nonHoverTextColor: textColor, nonHoverBgColor: baseButtonColor,
                            hoverTextColor: hoverTextColor, hoverBgColor: hoverButtonColor);

                if (!gameManager.Player.HasWaterJug())
                {
                    string noJugsMsg = "No Water Jugs!";
                    Vector2 msgSize = Raylib.MeasureTextEx(_mainFont, noJugsMsg, 16, 1);
                    Raylib.DrawTextEx(_mainFont, noJugsMsg,
                        new Vector2(_refillWaterButtonRect.X + (_refillWaterButtonRect.Width - msgSize.X) / 2,
                                    _refillWaterButtonRect.Y + _refillWaterButtonRect.Height + 5),
                        16, 1, Color.Orange);
                }
                if (!string.IsNullOrEmpty(machineContext.StatusMessage) && machineContext.AvailableActions.Contains("Refill Water"))
                {
                    Vector2 statusMsgSize = Raylib.MeasureTextEx(_mainFont, machineContext.StatusMessage, 16, 1);
                    Raylib.DrawTextEx(_mainFont, machineContext.StatusMessage,
                        new Vector2(_refillWaterButtonRect.X + (_refillWaterButtonRect.Width - statusMsgSize.X) / 2,
                                    _refillWaterButtonRect.Y - statusMsgSize.Y - 5),
                        16, 1, Color.Yellow);
                }
            }

            if (_currentWorkstationPhase == WorkstationPhase.SelectingIngredients)
            {
                drawShelf = true;
                drawTemplateAndItsContent = true;
            }
            else if (_currentWorkstationPhase == WorkstationPhase.AnimatingBrewActions)
            {
                drawShelf = false;
                actualTemplateDest.Y = _wsShelfDestRect.Y - 60f;
                actualTemplateDest.X = (_screenWidth - actualTemplateDest.Width) / 2f;
                drawTemplateAndItsContent = true;
                drawAnimationSlotsAndContent = true;
            }
            else if (_currentWorkstationPhase == WorkstationPhase.AwaitingServeOrTrashChoice)
            {
                drawShelf = false;
                drawTemplateAndItsContent = false;
                drawAnimationSlotsAndContent = false;
                drawCenteredServeUIWithCt2Bg = true;
            }

            string phaseString = $"WS Phase: {_currentWorkstationPhase.ToString()}";
            Vector2 phaseTextSize = Raylib.MeasureTextEx(_mainFont, phaseString, 20, 1);

            if (drawShelf) { DrawWorkstation_SelectingIngredients(gameManager); }

            float deltaX_template = actualTemplateDest.X - _wsTemplateDestRect.X;
            float deltaY_template = actualTemplateDest.Y - _wsTemplateDestRect.Y;

            if (drawTemplateAndItsContent)
            {
                Raylib.DrawTexturePro(_workstationSpritesheet1, _wsTemplateSourceRect, actualTemplateDest, Vector2.Zero, 0f, Color.White);

                for (int i = 0; i < MAX_SELECTED_INGREDIENTS; i++)
                {
                    if (i < _wsInputBoxVisualDestRects.Count)
                    {
                        Rectangle originalBoxDest = _wsInputBoxVisualDestRects[i];
                        Rectangle currentBoxDrawDest = new Rectangle(
                            originalBoxDest.X + deltaX_template, originalBoxDest.Y + deltaY_template,
                            originalBoxDest.Width, originalBoxDest.Height);
                        Raylib.DrawTexturePro(_workstationSpritesheet1, _wsInputBoxSourceRect, currentBoxDrawDest, Vector2.Zero, 0f, Color.White);

                        if (i < gameManager.Player.SelectedIngredientsForBrewing.Count)
                        {
                            string ingName = gameManager.Player.SelectedIngredientsForBrewing[i].Name;
                            Rectangle originalTextDest = _wsSelectedIngredientTextDestRects[i];
                            Rectangle currentTextDrawDest = new Rectangle(
                            originalTextDest.X + deltaX_template, originalTextDest.Y + deltaY_template,
                            originalTextDest.Width, originalTextDest.Height);
                            float fontSize = Math.Max(10, currentTextDrawDest.Height * 0.6f);
                            fontSize = Math.Min(fontSize, 20f);
                            Vector2 textSize = Raylib.MeasureTextEx(_mainFont, ingName, fontSize, 1);
                            Raylib.DrawTextEx(_mainFont, ingName,
                                new Vector2(currentTextDrawDest.X + (currentTextDrawDest.Width - textSize.X) / 2f,
                                            currentTextDrawDest.Y + (currentTextDrawDest.Height - textSize.Y) / 2f),
                                fontSize, 1, Color.Black);
                        }
                    }
                }

                Rectangle originalMeterAreaBgDest = _wsPropertyMeterAreaDestRect;
                Rectangle currentMeterAreaBgDrawDest = new Rectangle(
                    originalMeterAreaBgDest.X + deltaX_template, originalMeterAreaBgDest.Y + deltaY_template,
                    originalMeterAreaBgDest.Width, originalMeterAreaBgDest.Height);
                if (_wsPropertyMeterAreaSourceRect.Width > 0 && _wsPropertyMeterAreaSourceRect.Height > 0)
                {
                    Raylib.DrawTexturePro(_workstationSpritesheet1, _wsPropertyMeterAreaSourceRect, currentMeterAreaBgDrawDest, Vector2.Zero, 0f, Color.White);
                }
                else { Raylib.DrawRectangleRec(currentMeterAreaBgDrawDest, Color.DarkGray); }
                DrawPropertyMeters(gameManager, currentMeterAreaBgDrawDest);

                Rectangle originalResetDest = _wsResetButtonDestRect;
                Rectangle currentResetDrawDest = new Rectangle(
                    originalResetDest.X + deltaX_template, originalResetDest.Y + deltaY_template,
                    originalResetDest.Width, originalResetDest.Height);
                bool hoverReset = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), currentResetDrawDest);
                if (_workstationButtonSpritesheet.Id != 0)
                {
                    Raylib.DrawTexturePro(_workstationButtonSpritesheet, _wsResetButtonSourceRect, currentResetDrawDest, Vector2.Zero, 0f, hoverReset && _currentWorkstationPhase == WorkstationPhase.SelectingIngredients ? Color.LightGray : Color.White);
                }
                else { Raylib.DrawTexturePro(_workstationSpritesheet1, new Rectangle(612, 679, 109, 30), currentResetDrawDest, Vector2.Zero, 0f, hoverReset && _currentWorkstationPhase == WorkstationPhase.SelectingIngredients ? Color.LightGray : Color.White); }

                Rectangle originalBrewDest = _wsBrewButtonDestRect;
                Rectangle currentBrewDrawDest = new Rectangle(
                    originalBrewDest.X + deltaX_template, originalBrewDest.Y + deltaY_template,
                    originalBrewDest.Width, originalBrewDest.Height);
                bool hoverBrew = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), currentBrewDrawDest);
                Color brewButtonTint = Color.White;
                if (_currentWorkstationPhase == WorkstationPhase.SelectingIngredients)
                {
                    brewButtonTint = hoverBrew ? Color.LightGray : Color.White;
                }
                else
                {
                    brewButtonTint = Color.DarkGray;
                }
                if (_workstationButtonSpritesheet.Id != 0)
                {
                    Raylib.DrawTexturePro(_workstationButtonSpritesheet, _wsBrewButtonSourceRect, currentBrewDrawDest, Vector2.Zero, 0f, brewButtonTint);
                }
                else { Raylib.DrawTexturePro(_workstationSpritesheet1, new Rectangle(612, 709, 109, 30), currentBrewDrawDest, Vector2.Zero, 0f, brewButtonTint); }
            }

            if (drawAnimationSlotsAndContent)
            {
                DrawWorkstation_AnimatingBrewActions(gameManager);
            }

            if (drawCenteredServeUIWithCt2Bg)
            {
                float focusedAreaWidth = _screenWidth * 0.6f;
                float focusedAreaHeight = _screenHeight * 0.75f;

                if (_wsServingBgSourceRect.Width > 0 && _wsServingBgSourceRect.Height > 0)
                {
                    float sourceAspectRatio = _wsServingBgSourceRect.Width / _wsServingBgSourceRect.Height;
                    if (focusedAreaWidth / sourceAspectRatio <= focusedAreaHeight)
                    {
                        focusedAreaHeight = focusedAreaWidth / sourceAspectRatio;
                    }
                    else
                    {
                        focusedAreaWidth = focusedAreaHeight * sourceAspectRatio;
                    }
                }

                centeredServeArea = new Rectangle(
                    (_screenWidth - focusedAreaWidth) / 2f,
                    (_screenHeight - focusedAreaHeight) / 2f,
                    focusedAreaWidth,
                    focusedAreaHeight);

                DrawWorkstation_AwaitingServeOrTrash(gameManager, centeredServeArea);
            }

            // --- 3. DRAW WATER METER AND OVERHEAT/WATER JUG UI (On top of other elements) ---
            Equipment? currentStationEquipment = gameManager.ShopLayout.GetStation(EquipmentType.CoffeeMachine)?.AssignedEquipment;
            if (currentStationEquipment is CoffeeMachine mainBrewingStationInstance)
            {
                Raylib.DrawRectangleRec(_waterMeterBackgroundRect, Color.DarkGray);
                float waterFillPercent = 0f;
                if (mainBrewingStationInstance.WaterTankCapacityUnits > 0)
                {
                    waterFillPercent = (float)mainBrewingStationInstance.CurrentWaterUnits / mainBrewingStationInstance.WaterTankCapacityUnits;
                }
                waterFillPercent = Math.Clamp(waterFillPercent, 0f, 1f);
                float fillHeight = (float)Math.Floor(_waterMeterBackgroundRect.Height * waterFillPercent);
                Rectangle currentWaterFillRect = new Rectangle(
                    _waterMeterBackgroundRect.X,
                    _waterMeterBackgroundRect.Y + (_waterMeterBackgroundRect.Height - fillHeight),
                    _waterMeterBackgroundRect.Width,
                    fillHeight
                );
                Raylib.DrawRectangleRec(currentWaterFillRect, Color.SkyBlue);
                Raylib.DrawRectangleLinesEx(_waterMeterBackgroundRect, 2, Color.Black);
                string waterText = "Water";
                Vector2 waterTextSize = Raylib.MeasureTextEx(_mainFont, waterText, 10, 1);
                Raylib.DrawTextEx(_mainFont, waterText,
                    new Vector2(_waterMeterBackgroundRect.X + _waterMeterBackgroundRect.Width / 2 - waterTextSize.X / 2,
                                _waterMeterBackgroundRect.Y - waterTextSize.Y - 5),
                    10, 1, Color.White);
            }

            if (gameManager.IsBrewingStationOverheated)
            {
                Raylib.DrawRectangle(0, 0, _screenWidth, _screenHeight, new Color(180, 50, 50, 100));
                string overheatMsg = "STATION OVERHEATED!";
                Vector2 overheatMsgSize = Raylib.MeasureTextEx(_mainFont, overheatMsg, 28, 2);
                Raylib.DrawTextEx(_mainFont, overheatMsg,
                                new Vector2((_screenWidth - overheatMsgSize.X) / 2, _screenHeight / 2 - 80),
                                28, 2, Color.Orange);

                if (gameManager.Player.HasWaterJug())
                {
                    if (_waterJugTexture.Id != 0)
                    {
                        bool hoverUseJug = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), _useWaterJugButtonRect);
                        Raylib.DrawTexturePro(_waterJugTexture, new Rectangle(0, 0, _waterJugTexture.Width, _waterJugTexture.Height),
                                            _useWaterJugButtonRect, Vector2.Zero, 0f, hoverUseJug ? Color.LightGray : Color.White);
                    }
                    else
                    {
                        bool hoverUseJug = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), _useWaterJugButtonRect);
                        DrawTextButton("Use Jug", _useWaterJugButtonRect, hoverUseJug, 18, Color.White, hoverUseJug ? Color.SkyBlue : Color.Blue);
                    }
                    string jugCountText = $"x{gameManager.Player.WaterJugCount}";
                    Vector2 jugTextSize = Raylib.MeasureTextEx(_mainFont, jugCountText, 16, 1);
                    Raylib.DrawTextEx(_mainFont, jugCountText, new Vector2(_useWaterJugButtonRect.X + _useWaterJugButtonRect.Width + 5, _useWaterJugButtonRect.Y + (_useWaterJugButtonRect.Height - jugTextSize.Y) / 2), 16, 1, Color.White);
                }
                else
                {
                    if (_waterJugTexture.Id != 0) Raylib.DrawTexturePro(_waterJugTexture, new Rectangle(0, 0, _waterJugTexture.Width, _waterJugTexture.Height), _useWaterJugButtonRect, Vector2.Zero, 0f, new Color(100, 100, 100, 150));
                    else DrawTextButton("Use Jug", _useWaterJugButtonRect, false, 18, Color.DarkGray, Color.LightGray);

                    string noJugsMsg = "No Water Jugs!";
                    Vector2 noJugsSize = Raylib.MeasureTextEx(_mainFont, noJugsMsg, 16, 1);
                    Raylib.DrawTextEx(_mainFont, noJugsMsg, new Vector2(_useWaterJugButtonRect.X + (_useWaterJugButtonRect.Width - noJugsSize.X) / 2, _useWaterJugButtonRect.Y + _useWaterJugButtonRect.Height + 5), 16, 1, Color.Orange);
                }
            }

            string currentOrderTextOutput = "";
            if (_currentWorkstationPhase == WorkstationPhase.AwaitingServeOrTrashChoice && _workstationPreparedDrink != null)
            {
                currentOrderTextOutput = _workstationPreparedDrink.Name;
            }
            else if (gameManager.Player.CurrentOrder?.AssignedRecipe != null)
            {
                currentOrderTextOutput = $"Preparing: {gameManager.Player.CurrentOrder.AssignedRecipe.RecipeName}";
            }
            else
            {
                currentOrderTextOutput = (_currentWorkstationPhase == WorkstationPhase.SelectingIngredients) ? "Select Ingredients" : "No Active Order";
            }

            Vector2 orderTextSize = Raylib.MeasureTextEx(_mainFont, currentOrderTextOutput, 20, 1);
            float orderTextY = 10f;

            if (_currentWorkstationPhase == WorkstationPhase.SelectingIngredients)
            {
                orderTextY = _wsShelfDestRect.Y + _wsShelfDestRect.Height + 10;
            }
            else if (_currentWorkstationPhase == WorkstationPhase.AnimatingBrewActions)
            {
                orderTextY = actualTemplateDest.Y - orderTextSize.Y - 10;
            }
            else if (_currentWorkstationPhase == WorkstationPhase.AwaitingServeOrTrashChoice)
            {
                orderTextY = centeredServeArea.Y - orderTextSize.Y - 15;
                if (orderTextY < 10) orderTextY = 10;
            }

            if (!string.IsNullOrEmpty(currentOrderTextOutput) && orderTextY > phaseTextSize.Y + 5 && orderTextY < _screenHeight - orderTextSize.Y - 5)
            {
                Raylib.DrawTextEx(_mainFont, currentOrderTextOutput, new Vector2((_screenWidth - orderTextSize.X) / 2f, orderTextY), 20, 1, Color.Yellow);
            }

            if (!string.IsNullOrEmpty(_workstationMessage) && _workstationMessageTimer > 0)
            {
                Color messageColor = _workstationMessageIsError ? Color.Red : new Color(200, 255, 200, 255);
                Vector2 messageSize = Raylib.MeasureTextEx(_mainFont, _workstationMessage, 20, 1);
                float messageY = _screenHeight - 50f;

                if (_currentWorkstationPhase == WorkstationPhase.AwaitingServeOrTrashChoice)
                {
                    messageY = centeredServeArea.Y + centeredServeArea.Height + 10;
                    if (messageY + messageSize.Y > _screenHeight - 10) messageY = _screenHeight - messageSize.Y - 10;
                }
                messageY = Math.Max(orderTextY + orderTextSize.Y + 5, messageY);
                messageY = Math.Min(messageY, _screenHeight - messageSize.Y - 10);
                Raylib.DrawTextEx(_mainFont, _workstationMessage, new Vector2((_screenWidth - messageSize.X) / 2f, messageY), 20, 1, messageColor);
            }

            // At first, I wanted to draw the sanity meter in the workstation screen, but it was too packed, so i chose not to
            // // --- 3. DRAW PLAYER'S SANITY METER (Top-Left, using _sanityMeterScreenPos) ---
            // if (_sanityMeterTexture.Id != 0 && _sanityMeterBorderSourceRect.Width > 0)
            // {
            //     Raylib.DrawTextureRec(_sanityMeterTexture, _sanityMeterBorderSourceRect, _sanityMeterScreenPos, Color.White); 
            //     float sanity = gameManager.Player.SanityLevel;
            //     int totalSegments = 5;
            //     int visibleSegments;
            //     if (sanity > 80) visibleSegments = 5;
            //     else if (sanity > 60) visibleSegments = 4;
            //     else if (sanity > 40) visibleSegments = 3;
            //     else if (sanity > 20) visibleSegments = 2;
            //     else if (sanity > 0) visibleSegments = 1; else visibleSegments = 0;
            //     visibleSegments = Math.Clamp(visibleSegments, 0, totalSegments);

            //     float segmentWidthSource = _sanityMeterFillSourceBaseRect.Width / totalSegments;
            //     float currentFillWidthSource = segmentWidthSource * visibleSegments;

            //     if (currentFillWidthSource > 0)
            //     {
            //         Rectangle currentSanityFillSourceRect = new Rectangle(_sanityMeterFillSourceBaseRect.X, _sanityMeterFillSourceBaseRect.Y, currentFillWidthSource, _sanityMeterFillSourceBaseRect.Height);
            //         Vector2 fillScreenPos = new Vector2(
            //             _sanityMeterScreenPos.X + (_sanityMeterFillSourceBaseRect.X - _sanityMeterBorderSourceRect.X),
            //             _sanityMeterScreenPos.Y + (_sanityMeterFillSourceBaseRect.Y - _sanityMeterBorderSourceRect.Y) - 38
            //         );
            //         Raylib.DrawTextureRec(_sanityMeterTexture, currentSanityFillSourceRect, fillScreenPos, Color.White);
            //     }

            //     //Raylib.DrawTextEx(_mainFont, $"Sanity: {sanity:F0}%", new Vector2(_sanityMeterScreenPos.X + _sanityMeterBorderSourceRect.Width + 5, _sanityMeterScreenPos.Y + (_sanityMeterBorderSourceRect.Height / 2f) - 9f), 18, 1, Color.White);
            // }

            // --- 4. DRAW CURRENT CUSTOMER'S PATIENCE METER (Top-Left, below Sanity Meter, using _patienceMeterScreenPos) ---
            if (_patienceMeterTexture.Id != 0 && gameManager.Player.CurrentOrder?.RequestingCustomer != null)
            {
                Customer currentOrderCustomer = gameManager.Player.CurrentOrder.RequestingCustomer;
                float patiencePercent = currentOrderCustomer.Patience / Customer.INITIAL_PATIENCE;
                patiencePercent = Math.Clamp(patiencePercent, 0.0f, 1.0f);

                int frameIndex = MAX_PATIENCE_FRAMES - 1 - (int)Math.Round(patiencePercent * (MAX_PATIENCE_FRAMES - 1));
                frameIndex = Math.Clamp(frameIndex, 0, MAX_PATIENCE_FRAMES - 1);

                if (frameIndex < _patienceMeterFrames.Length)
                {
                    Raylib.DrawTextureRec(_patienceMeterTexture, _patienceMeterFrames[frameIndex], _patienceMeterScreenPos, Color.White);
                    // Raylib.DrawTextEx(_mainFont, $"Patience: {currentOrderCustomer.Patience:F0}%", new Vector2(_patienceMeterScreenPos.X + _patienceMeterFrames[0].Width + 5, _patienceMeterScreenPos.Y + 5), 18, 1, Color.White);
                }
            }
        }

        private void DrawWorkstation_SelectingIngredients(GameManager gameManager)
        {
            Vector2 mousePos = this.GameCursorPosition;
            Raylib.DrawTexturePro(_workstationSpritesheet1, _wsShelfSourceRect, _wsShelfDestRect, Vector2.Zero, 0f, Color.White);

            for (int i = 0; i < _wsIngredientDisplayDestRectsOnShelf.Count && i < _displayableIngredientTypesOnShelf.Count; i++)
            {
                IngredientType type = _displayableIngredientTypesOnShelf[i];
                bool isUnlocked = gameManager.GloballyUnlockedIngredientTypes.Contains(type);
                
                // FIX: Use the correct public property IsIngredientCurrentlyStolen
                bool isStolen = gameManager.IsIngredientCurrentlyStolen && gameManager.StolenIngredientType == type;
                
                Color tint = isStolen ? new Color(120, 120, 120, 100) : Color.White;

                bool isHovered = Raylib.CheckCollisionPointRec(mousePos, _wsIngredientDisplayDestRectsOnShelf[i]);
                Rectangle sourceRect;

                if (!isUnlocked)
                {
                    Rectangle iconRect = _wsIngredientDisplayDestRectsOnShelf[i];
                    Color lockedOverlayColor = new Color(0, 0, 0, 150); 
                    Raylib.DrawRectangleRec(iconRect, lockedOverlayColor);
                    string lockedText = "LOCKED";
                    Vector2 lockedTextSize = Raylib.MeasureTextEx(_mainFont, lockedText, 14, 2);
                    Raylib.DrawTextEx(_mainFont, lockedText, 
                        new Vector2(iconRect.X + (iconRect.Width - lockedTextSize.X) / 2, iconRect.Y + (iconRect.Height - lockedTextSize.Y) / 2),
                        14, 2, Color.White);
                }
                else if (isStolen)
                {
                    Rectangle iconRect = _wsIngredientDisplayDestRectsOnShelf[i];
                    string stolenText = "STOLEN!";
                    int fontSize = Math.Max(12, (int)(iconRect.Height * 0.25f));
                    fontSize = Math.Min(fontSize, 18);
                    Vector2 textSize = Raylib.MeasureTextEx(_mainFont, stolenText, fontSize, 1);
                    Raylib.DrawRectangleRec(iconRect, new Color(255, 0, 0, 70));
                    Raylib.DrawTextEx(_mainFont, stolenText, new Vector2(iconRect.X + (iconRect.Width - textSize.X) / 2, iconRect.Y + (iconRect.Height - textSize.Y) / 2), fontSize, 1, Color.Yellow);
                }

                if (isHovered && _wsIngredientHoverIconSourceRects.TryGetValue(type, out var hoverRect)) { sourceRect = hoverRect; }
                else if (_wsIngredientNormalIconSourceRects.TryGetValue(type, out var normalRect)) { sourceRect = normalRect; }
                else { continue; }

                Raylib.DrawTexturePro(_workstationSpritesheet1, sourceRect, _wsIngredientDisplayDestRectsOnShelf[i], Vector2.Zero, 0f, tint);
            }
        }

        private void DrawPropertyMeters(GameManager gameManager, Rectangle meterAreaEffectiveScreenDest)
        {
            if (_mainFont.Texture.Id == 0) return;

            int totalBitterness = 0, totalSourness = 0, totalSweetness = 0, totalToxicity = 0;
            foreach (var ing in gameManager.Player.SelectedIngredientsForBrewing)
            {
                totalBitterness += ing.Bitterness; totalSourness += ing.Sourness;
                totalSweetness += ing.Sweetness;
            }
            totalBitterness = Math.Max(0, Math.Min(NUM_METER_SEGMENTS, totalBitterness));
            totalSourness = Math.Max(0, Math.Min(NUM_METER_SEGMENTS, totalSourness));
            totalSweetness = Math.Max(0, Math.Min(NUM_METER_SEGMENTS, totalSweetness));
            totalToxicity = Math.Max(0, Math.Min(NUM_METER_SEGMENTS, totalToxicity));

            var propertiesToDraw = new Dictionary<string, int> {
                { "BITTER", totalBitterness }, { "SWEET", totalSweetness }, { "SOUR", totalSourness }
            };
            bool showToxic = (gameManager.CurrentLevel != null && gameManager.CurrentLevel.LevelID.Contains("10"));
            if (showToxic) { propertiesToDraw.Add("TOXIC", totalToxicity); }

            float deltaX_from_original_meter_area = meterAreaEffectiveScreenDest.X - _wsPropertyMeterAreaDestRect.X;
            float deltaY_from_original_meter_area = meterAreaEffectiveScreenDest.Y - _wsPropertyMeterAreaDestRect.Y;

            float desiredLabelBaseFontSize = 10f;
            float currentTemplateDisplayScale = meterAreaEffectiveScreenDest.Width / (_wsPropertyMeterAreaSourceRect.Width > 0 ? _wsPropertyMeterAreaSourceRect.Width : 109f);
            float scaledLabelFontSize = Math.Max(7f, desiredLabelBaseFontSize * currentTemplateDisplayScale);
            scaledLabelFontSize = Math.Min(scaledLabelFontSize, 12f);

            foreach (var propEntry in propertiesToDraw)
            {
                string propNameUpper = propEntry.Key.ToUpperInvariant();
                int propValue = propEntry.Value;

                if (_wsPropertyMeterBoxesDestRects.TryGetValue(propNameUpper, out List<Rectangle>? originalSegmentDests) && originalSegmentDests.Any())
                {
                    Rectangle firstOriginalSegment = originalSegmentDests[0];
                    float segmentsActualDrawY = firstOriginalSegment.Y + deltaY_from_original_meter_area;
                    float segmentActualSquareSize = firstOriginalSegment.Width;

                    Vector2 labelTextSize = Raylib.MeasureTextEx(_mainFont, propNameUpper, scaledLabelFontSize, 1);
                    float labelDrawX = meterAreaEffectiveScreenDest.X + (meterAreaEffectiveScreenDest.Width - labelTextSize.X) / 2f;

                    float basePaddingBelowLabel = 2f;
                    float scaledPaddingBelowLabel = basePaddingBelowLabel * currentTemplateDisplayScale;
                    float labelDrawY = segmentsActualDrawY - scaledPaddingBelowLabel - labelTextSize.Y;

                    Raylib.DrawTextEx(_mainFont, propNameUpper, new Vector2(labelDrawX, labelDrawY), scaledLabelFontSize, 1, Color.DarkGray);

                    for (int i = 0; i < NUM_METER_SEGMENTS; i++)
                    {
                        if (i < originalSegmentDests.Count)
                        {
                            Rectangle originalSegmentDest = originalSegmentDests[i];
                            Rectangle currentSegmentDrawDest = new Rectangle(
                                originalSegmentDest.X + deltaX_from_original_meter_area,
                                segmentsActualDrawY,
                                segmentActualSquareSize,
                                segmentActualSquareSize
                            );

                            Color segmentBgColor = Color.LightGray;
                            Color segmentFillColor = Color.Black;

                            if (_whiteBoxTexture.Id != 0 && _blackBoxTexture.Id != 0)
                            {
                                Raylib.DrawTexturePro(_whiteBoxTexture, new Rectangle(0, 0, _whiteBoxTexture.Width, _whiteBoxTexture.Height), currentSegmentDrawDest, Vector2.Zero, 0f, Color.White);
                                if (i < propValue)
                                {
                                    Raylib.DrawTexturePro(_blackBoxTexture, new Rectangle(0, 0, _blackBoxTexture.Width, _blackBoxTexture.Height), currentSegmentDrawDest, Vector2.Zero, 0f, Color.White);
                                }
                            }
                            else
                            {
                                Raylib.DrawRectangleRec(currentSegmentDrawDest, segmentBgColor);
                                if (i < propValue)
                                {
                                    Raylib.DrawRectangleRec(currentSegmentDrawDest, segmentFillColor);
                                }
                            }
                            Raylib.DrawRectangleLinesEx(currentSegmentDrawDest, 1, Color.DarkGray);
                        }
                    }
                }
            }
        }
        private void DrawWorkstation_AnimatingBrewActions(GameManager gameManager)
        {
            if (_workstationSpritesheet1.Id == 0 && _wsIngredientActionAnimSourceRects.Count == 0)
            {
                if (_wsBrewActionAnimDestRects.Any())
                {
                    Raylib.DrawText("Animation resources missing", (int)_wsBrewActionAnimDestRects[0].X, (int)_wsBrewActionAnimDestRects[0].Y - 20, 10, Color.Red);
                }
                return;
            }

            if (gameManager.Player.SelectedIngredientsForBrewing.Count == 0)
            {
                for (int i = 0; i < MAX_SELECTED_INGREDIENTS && i < _wsBrewActionAnimDestRects.Count; i++)
                {
                    Rectangle destSquare = _wsBrewActionAnimDestRects[i];
                    Raylib.DrawTextEx(_mainFont, $"Slot {i + 1}", new Vector2(destSquare.X + 5, destSquare.Y + 5), 10, 1, Color.White);
                }
                return;
            }

            for (int i = 0; i < MAX_SELECTED_INGREDIENTS; ++i)
            {
                if (i < _wsBrewActionAnimDestRects.Count)
                {
                    Rectangle destSquare = _wsBrewActionAnimDestRects[i];

                    if (i < gameManager.Player.SelectedIngredientsForBrewing.Count)
                    {
                        Ingredient currentIngredient = gameManager.Player.SelectedIngredientsForBrewing[i];

                        if (_currentWorkstationPhase == WorkstationPhase.AnimatingBrewActions)
                        {
                            Color tint = Color.White;
                            if (i == _currentBrewActionIndex)
                            {
                                if (_wsIngredientActionAnimSourceRects.TryGetValue(currentIngredient.Type, out var animSourceRect))
                                { Raylib.DrawTexturePro(_workstationSpritesheet1, animSourceRect, destSquare, Vector2.Zero, 0f, tint); }
                                else { Raylib.DrawRectangleRec(destSquare, Color.Purple); Raylib.DrawTextEx(_mainFont, "NoAnim", new Vector2(destSquare.X + 2, destSquare.Y + 2), Math.Max(10, destSquare.Height / 3), 1, Color.White); }
                            }
                            else if (i < _currentBrewActionIndex)
                            {
                                if (_wsIngredientActionAnimSourceRects.TryGetValue(currentIngredient.Type, out var animSourceRect))
                                { Raylib.DrawTexturePro(_workstationSpritesheet1, animSourceRect, destSquare, Vector2.Zero, 0f, Color.Gray); }
                                else { Raylib.DrawRectangleRec(destSquare, Color.DarkGray); Raylib.DrawTextEx(_mainFont, "Done", new Vector2(destSquare.X + 2, destSquare.Y + 2), Math.Max(10, destSquare.Height / 3), 1, Color.LightGray); }
                            }
                            else
                            {
                                if (_wsIngredientNormalIconSourceRects.TryGetValue(currentIngredient.Type, out var iconSourceRect))
                                { Raylib.DrawTexturePro(_workstationSpritesheet1, iconSourceRect, destSquare, Vector2.Zero, 0f, new Color(100, 100, 100, 100)); }
                                else { Raylib.DrawRectangleRec(destSquare, new Color(50, 50, 50, 100)); }
                            }
                        }
                        else if (_currentWorkstationPhase == WorkstationPhase.AwaitingServeOrTrashChoice)
                        {
                            if (_wsIngredientNormalIconSourceRects.TryGetValue(currentIngredient.Type, out var iconSourceRect))
                            {
                                Raylib.DrawTexturePro(_workstationSpritesheet1, iconSourceRect, destSquare, Vector2.Zero, 0f, Color.White);
                                Raylib.DrawTextEx(_mainFont, "✓", new Vector2(destSquare.X + destSquare.Width - 12, destSquare.Y + destSquare.Height - 12), Math.Max(10, destSquare.Height / 2.5f), 1, Color.Lime);
                            }
                            else { Raylib.DrawRectangleRec(destSquare, Color.LightGray); Raylib.DrawTextEx(_mainFont, "Used", new Vector2(destSquare.X + 2, destSquare.Y + 2), Math.Max(10, destSquare.Height / 3), 1, Color.LightGray); }
                        }
                    }
                    else
                    {
                        Raylib.DrawRectangleRec(destSquare, new Color(40, 40, 40, 150));
                    }
                }
            }

            if (_currentWorkstationPhase == WorkstationPhase.AnimatingBrewActions)
            {
                string brewingText = "Brewing";
                int dots = ((int)(_brewActionAnimationTimer * 2.5f)) % 4;
                for (int d = 0; d < dots; ++d) brewingText += ".";
                float textYPos = _screenHeight - 40;
                if (_wsBrewActionAnimDestRects.Any())
                {
                    textYPos = _wsBrewActionAnimDestRects.Last().Y + _wsBrewActionAnimDestRects.Last().Height + 20; 
                }
                textYPos = Math.Min(textYPos, _screenHeight - 40);
                Vector2 brewingTextSize = Raylib.MeasureTextEx(_mainFont, brewingText, 20, 1);
                Raylib.DrawTextEx(_mainFont, brewingText,
                    new Vector2((_screenWidth - brewingTextSize.X) / 2f, textYPos),
                    20, 1, Color.Yellow);
            }
        }

        private void DrawWorkstation_AwaitingServeOrTrash(GameManager gameManager, Rectangle targetDrawingArea)
        {
            if (_workstationSpritesheet2.Id == 0 ||
                _serveButtonTexture.Id == 0 || _trashButtonTexture.Id == 0 ||
                _mainFont.Texture.Id == 0)
            {
                Raylib.DrawRectangleRec(targetDrawingArea, Color.DarkGray);
                if (_mainFont.Texture.Id != 0) Raylib.DrawTextEx(_mainFont, "Serve UI Assets Missing!",
                    new Vector2(targetDrawingArea.X + 10, targetDrawingArea.Y + 10), 20, 1, Color.Red);
                return;
            }

            Raylib.DrawTexturePro(
                _workstationSpritesheet2,
                _wsServingBgSourceRect,
                targetDrawingArea,
                Vector2.Zero, 0f, Color.White);

            float elementScale = targetDrawingArea.Height / (_wsServingBgSourceRect.Height > 0 ? _wsServingBgSourceRect.Height : targetDrawingArea.Height);
            if (_wsServingBgSourceRect.Height == 0) elementScale = 0.5f;

            float handDrawWidth = _wsHandSourceRect.Width * elementScale * 1.0f;
            float handDrawHeight = _wsHandSourceRect.Height * elementScale * 1.0f;
            float handBaseX = targetDrawingArea.X + (targetDrawingArea.Width - handDrawWidth) / 2f;
            float handShiftRightAmount = 25f * elementScale;
            float handVerticalPlacementFactor = 1.0f;

            Rectangle handDest = new Rectangle(
                handBaseX + handShiftRightAmount,
                targetDrawingArea.Y + (targetDrawingArea.Height - handDrawHeight) * handVerticalPlacementFactor,
                handDrawWidth,
                handDrawHeight);

            float cupDrawWidth = _wsCupSourceRect.Width * elementScale * 0.9f;
            float cupDrawHeight = _wsCupSourceRect.Height * elementScale * 0.9f;
            float cupHorizontalCenteringOffset = (handDest.Width - cupDrawWidth) * 0.5f;
            float cupManualShiftX = -53f * elementScale;
            float cupVerticalAnchorFactorOnHand = 0.15f;
            float cupVerticalFineTuneFactor = -0.05f;

            Rectangle cupDest = new Rectangle(
                handDest.X + cupHorizontalCenteringOffset + cupManualShiftX,
                handDest.Y + handDest.Height * cupVerticalAnchorFactorOnHand + cupDrawHeight * cupVerticalFineTuneFactor,
                cupDrawWidth, cupDrawHeight);

            Rectangle preparedDrinkDest = new Rectangle(
                cupDest.X + cupDest.Width * 0.03f,
                cupDest.Y + cupDest.Height * -0.45f,
                cupDest.Width, cupDest.Height);

            float desiredButtonHeightOnScreen = _screenHeight * 0.12f;
            float screenEdgePadding = 40f;
            Vector2 mousePos = Raylib.GetMousePosition();

            Rectangle serveBtnDest = new Rectangle();
            bool hoverServe = false;

            if (_serveButtonTexture.Id != 0)
            {
                Rectangle serveButtonSource = new Rectangle(0, 0, _serveButtonTexture.Width, _serveButtonTexture.Height);
                float serveButtonAspectRatio = (_serveButtonTexture.Height > 0) ? (float)_serveButtonTexture.Width / _serveButtonTexture.Height : 1f;
                float serveBtnH = desiredButtonHeightOnScreen;
                float serveBtnW = serveBtnH * serveButtonAspectRatio;

                serveBtnDest = new Rectangle(
                    _screenWidth - screenEdgePadding - serveBtnW,
                    (_screenHeight - serveBtnH) / 2f,
                    serveBtnW, serveBtnH);

                hoverServe = Raylib.CheckCollisionPointRec(mousePos, serveBtnDest);
                Raylib.DrawTexturePro(_serveButtonTexture, new Rectangle(0, 0, _serveButtonTexture.Width, _serveButtonTexture.Height), serveBtnDest, Vector2.Zero, 0f, hoverServe ? Color.LightGray : Color.White);
            }
            else
            {
                serveBtnDest = new Rectangle(_screenWidth - screenEdgePadding - 70, (_screenHeight - 30) / 2f, 70, 30);
                hoverServe = Raylib.CheckCollisionPointRec(mousePos, serveBtnDest);
                Raylib.DrawRectangleRec(serveBtnDest, hoverServe ? Color.DarkGreen : Color.Green);
                if (_mainFont.Texture.Id != 0) Raylib.DrawText("SERVE", (int)serveBtnDest.X + 10, (int)serveBtnDest.Y + 7, 20, Color.White);
            }

            Rectangle trashBtnDest = new Rectangle();
            bool hoverTrash = false;

            if (_trashButtonTexture.Id != 0)
            {
                Rectangle trashButtonSource = new Rectangle(0, 0, _trashButtonTexture.Width, _trashButtonTexture.Height);
                float trashButtonAspectRatio = (_trashButtonTexture.Height > 0) ? (float)_trashButtonTexture.Width / _trashButtonTexture.Height : 1f;
                float trashBtnH = desiredButtonHeightOnScreen;
                float trashBtnW = trashBtnH * trashButtonAspectRatio;

                trashBtnDest = new Rectangle(
                    screenEdgePadding,
                    (_screenHeight - trashBtnH) / 2f,
                    trashBtnW, trashBtnH);

                hoverTrash = Raylib.CheckCollisionPointRec(mousePos, trashBtnDest);
                Raylib.DrawTexturePro(_trashButtonTexture, new Rectangle(0, 0, _trashButtonTexture.Width, _trashButtonTexture.Height), trashBtnDest, Vector2.Zero, 0f, hoverTrash ? Color.LightGray : Color.White);
            }
            else
            {
                trashBtnDest = new Rectangle(screenEdgePadding, (_screenHeight - 30) / 2f, 70, 30);
                hoverTrash = Raylib.CheckCollisionPointRec(mousePos, trashBtnDest);
                Raylib.DrawRectangleRec(trashBtnDest, hoverTrash ? Color.Red : Color.Red);
                if (_mainFont.Texture.Id != 0) Raylib.DrawText("TRASH", (int)trashBtnDest.X + 10, (int)trashBtnDest.Y + 7, 20, Color.White);
            }

            _wsServeButtonDestRect = serveBtnDest;
            _wsTrashButtonDestRect = trashBtnDest;

            if (_workstationSpritesheet2.Id != 0)
            {
                Raylib.DrawTexturePro(_workstationSpritesheet2, _wsHandSourceRect, handDest, Vector2.Zero, 0f, Color.White);
                Raylib.DrawTexturePro(_workstationSpritesheet2, _wsCupSourceRect, cupDest, Vector2.Zero, 0f, Color.White);

                if (_workstationBrewSuccess && _workstationPreparedDrink != null)
                {
                    if (_wsDrinkOutputSourceRects.TryGetValue(_workstationPreparedDrink.BaseRecipe.RecipeName, out var drinkSpriteRect))
                    {
                        Raylib.DrawTexturePro(_workstationSpritesheet2, drinkSpriteRect, preparedDrinkDest, Vector2.Zero, 0f, Color.White);
                    }
                    else { Raylib.DrawRectangleRec(preparedDrinkDest, Color.DarkGreen); }
                }
                else
                {
                    Raylib.DrawRectangleRec(preparedDrinkDest, new Color(100, 30, 30, 255));
                }
            }
            else
            {
                Raylib.DrawRectangleRec(handDest, Color.Gray);
                Raylib.DrawRectangleRec(cupDest, Color.LightGray);
                if (_workstationBrewSuccess && _workstationPreparedDrink != null)
                {
                    Raylib.DrawRectangleRec(preparedDrinkDest, Color.DarkGreen);
                }
                else { Raylib.DrawRectangleRec(preparedDrinkDest, new Color(100, 30, 30, 255)); }
            }
        }

        public void DrawGameOverScreen(GameManager gameManager)
        {
            Raylib.DrawRectangle(0, 0, _screenWidth, _screenHeight, new Color(100, 0, 0, 200));
            string gameOverText = "GAME OVER";
            string scoreText = $"Final Score: {gameManager.PlayerScore}";
            string continueText = "Press ENTER or Click to return to Main Menu";

            Vector2 gameOverSize = Raylib.MeasureTextEx(_mainFont, gameOverText, 70, 2);
            Vector2 scoreSize = Raylib.MeasureTextEx(_mainFont, scoreText, 40, 1);
            Vector2 continueSize = Raylib.MeasureTextEx(_mainFont, continueText, 20, 1);

            Raylib.DrawTextEx(_mainFont, gameOverText, new Vector2(_screenWidth / 2f - gameOverSize.X / 2f, _screenHeight / 2f - gameOverSize.Y - 20), 70, 2, Color.Yellow);
            Raylib.DrawTextEx(_mainFont, scoreText, new Vector2(_screenWidth / 2f - scoreSize.X / 2f, _screenHeight / 2f + 10), 40, 1, Color.White);
            Raylib.DrawTextEx(_mainFont, continueText, new Vector2(_screenWidth / 2f - continueSize.X / 2f, _screenHeight / 2f + scoreSize.Y + 30), 20, 1, Color.LightGray);
        }

        public void TransitionToScreen(GameView newView)
        {
            if ((_isTransitioning && _currentGameView == newView) || (!_isTransitioning && _currentGameView == newView))
            {
                // Console.WriteLine($"[UIManager.TransitionToScreen] Already in view {newView} or transition in progress. No change.");
                return;
            }

            _isTransitioning = true;
            Console.WriteLine($"UIManager: Transitioning from {_currentGameView} to {newView}");

            _previousView = _currentGameView;
            _currentGameView = newView;
            _lastClickProcessedTime = 0f; 

            if (newView == GameView.WorkstationScreen)
            {
                if (_gameManagerInstance != null)
                {
                    ResetWorkstationToSelection(_gameManagerInstance);
                }
                else
                {
                    Console.WriteLine("ERROR: UIManager.TransitionToScreen - _gameManagerInstance is null. Cannot fully reset workstation (player ingredients might not clear).");
                    _currentWorkstationPhase = WorkstationPhase.SelectingIngredients;
                    _workstationMessage = string.Empty;
                    _workstationPreparedDrink = null;
                    _brewActionAnimationTimer = 0f;
                    _currentBrewActionIndex = 0;
                    Console.WriteLine("UIManager: Workstation UI elements reset, but player ingredients NOT cleared due to missing GameManager instance.");
                }
            }
            else if (newView == GameView.DialogueScreen)
            {
                _dialoguePortraitFrameTimer = 0f;
                if (_gameManagerInstance?.CurrentlyInteractingCustomer != null)
                {
                    _gameManagerInstance.CurrentlyInteractingCustomer.ResetDialogueFrame();
                }
            }
            _isTransitioning = false;

        }

        public void ShowDialogue(string message)
        {
            Console.WriteLine($"UIManager.ShowDialogue (simple message): {message}");
            SetWorkstationMessage(message, false);
        }

        public void DisplayMessage(string message, MessageType type, bool useWorkstationDisplay = false)
        {
            Console.WriteLine($"UI Message [{type}]: {message}");
            if (useWorkstationDisplay || _currentGameView == GameView.WorkstationScreen)
            {
                SetWorkstationMessage(message, type == MessageType.Error || type == MessageType.Warning);
            }
        }

        public void DrawShopScreen(GameManager gameManager)
        {
            if (_shopBackgroundTexture.Id != 0)
            {
                Raylib.DrawTexturePro(_shopBackgroundTexture,
                    new Rectangle(0, 0, _shopBackgroundTexture.Width, _shopBackgroundTexture.Height),
                    new Rectangle(0, 0, _screenWidth, _screenHeight), Vector2.Zero, 0f, Color.White);
            }
            else
            {
                Raylib.ClearBackground(new Color(30, 40, 50, 255));
            }

            string title = "Convenience Store";
            Vector2 titleSize = Raylib.MeasureTextEx(_mainFont, title, 30, 2);
            Raylib.DrawTextEx(_mainFont, title, new Vector2((_screenWidth - titleSize.X) / 2, 20), 30, 2, Color.SkyBlue);
            Raylib.DrawTextEx(_mainFont, $"Your Money: ${gameManager.Player.Money:F2}", new Vector2(50, 70), 20, 1, Color.Gold);

            _shopIngredientLayout.Clear();
            float shopListAreaX = 50;
            float shopListWidth = _screenWidth - 100;
            float shopListVisibleHeight = _screenHeight - SHOP_LIST_START_Y - 90;

            Raylib.BeginScissorMode((int)shopListAreaX, (int)SHOP_LIST_START_Y, (int)shopListWidth, (int)shopListVisibleHeight);

            List<Ingredient> displayIngredients = gameManager.AllDefinedIngredients.OrderBy(ing => ing.Name).ToList();
            float itemEntryY = SHOP_LIST_START_Y + _shopScrollOffset.Y;

            if (!displayIngredients.Any() && gameManager.Player.WaterJugCount == 0)
            {
                Raylib.DrawTextEx(_mainFont, "Shop is currently empty.",
                    new Vector2(shopListAreaX + 10, itemEntryY), 20, 1, Color.Orange);
            }

            // --- Draw Ingredients ---
            foreach (Ingredient ing in displayIngredients)
            {
                if (itemEntryY + SHOP_ITEM_ENTRY_HEIGHT >= SHOP_LIST_START_Y &&
                    itemEntryY <= SHOP_LIST_START_Y + shopListVisibleHeight)
                {
                    int currentStock = gameManager.Player.Inventory.TryGetValue(ing, out int stock) ? stock : 0;
                    bool isUnlocked = gameManager.GloballyUnlockedIngredientTypes.Contains(ing.Type);
                    Color itemTextColor = isUnlocked ? Color.White : Color.Gray;
                    string itemText = $"{ing.Name} (Cost: ${ing.Cost:F2}) - Stock: {currentStock}";
                    if (!isUnlocked) itemText += " (Locked)";

                    float buttonGroupTotalWidth = (SHOP_BUTTON_WIDTH * 2) + 10;
                    float rightPaddingForButtons = 10;
                    float buttonsXStart = shopListAreaX + shopListWidth - buttonGroupTotalWidth - rightPaddingForButtons;
                    float availableTextWidth = buttonsXStart - (shopListAreaX + 10) - 5;

                    string textToDraw = itemText;
                    Vector2 itemTextSize = Raylib.MeasureTextEx(_mainFont, textToDraw, 20, 1);
                    if (itemTextSize.X > availableTextWidth)
                    {
                        int numChars = (int)(textToDraw.Length * (availableTextWidth / itemTextSize.X));
                        textToDraw = textToDraw.Substring(0, Math.Max(0, numChars - 3)) + "...";
                    }

                    Raylib.DrawTextEx(_mainFont, textToDraw,
                        new Vector2(shopListAreaX + 10, itemEntryY + (SHOP_ITEM_ENTRY_HEIGHT - 20) / 2),
                        20, 1, itemTextColor);

                    Rectangle buy1ButtonRect = new Rectangle(buttonsXStart, itemEntryY + (SHOP_ITEM_ENTRY_HEIGHT - SHOP_BUTTON_HEIGHT) / 2, SHOP_BUTTON_WIDTH, SHOP_BUTTON_HEIGHT);
                    Rectangle buy10ButtonRect = new Rectangle(buttonsXStart + SHOP_BUTTON_WIDTH + 10, itemEntryY + (SHOP_ITEM_ENTRY_HEIGHT - SHOP_BUTTON_HEIGHT) / 2, SHOP_BUTTON_WIDTH, SHOP_BUTTON_HEIGHT);

                    if (isUnlocked)
                    {
                        DrawTextButton($"Buy 1", buy1ButtonRect, Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), buy1ButtonRect), 16, Color.Black, Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), buy1ButtonRect) ? new Color(180, 220, 255, 255) : new Color(150, 200, 255, 255));
                        DrawTextButton($"Buy 10", buy10ButtonRect, Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), buy10ButtonRect), 16, Color.Black, Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), buy10ButtonRect) ? new Color(180, 220, 255, 255) : new Color(150, 200, 255, 255));
                    }
                    else
                    {
                        DrawTextButton($"Buy 1", buy1ButtonRect, false, 16, Color.DarkGray, new Color(80, 80, 80, 200));
                        DrawTextButton($"Buy 10", buy10ButtonRect, false, 16, Color.DarkGray, new Color(80, 80, 80, 200));
                    }
                    _shopIngredientLayout.Add(new Tuple<Ingredient, Rectangle, Rectangle>(ing, buy1ButtonRect, buy10ButtonRect));
                }
                itemEntryY += SHOP_ITEM_ENTRY_HEIGHT + SHOP_LIST_ITEM_PADDING;
            }

            // --- Draw Water Jug Purchase UI ---
            float waterJugCost = 5.0f;
            string waterJugName = "Water Jug";
            string waterJugText = $"{waterJugName} (Cost: ${waterJugCost:F2}) - Have: {gameManager.Player.WaterJugCount}";

            if (itemEntryY + SHOP_ITEM_ENTRY_HEIGHT >= SHOP_LIST_START_Y &&
                itemEntryY <= SHOP_LIST_START_Y + shopListVisibleHeight)
            {
                float buttonGroupTotalWidth = (SHOP_BUTTON_WIDTH * 2) + 10;
                float rightPaddingForButtons = 10;
                float buttonsXStart_jug = shopListAreaX + shopListWidth - buttonGroupTotalWidth - rightPaddingForButtons;
                float availableTextWidth_jug = buttonsXStart_jug - (shopListAreaX + 10) - 5;

                string textToDraw_jug = waterJugText;
                Vector2 waterJugTextSize = Raylib.MeasureTextEx(_mainFont, textToDraw_jug, 20, 1);
                if (waterJugTextSize.X > availableTextWidth_jug)
                {
                    int numChars = (int)(textToDraw_jug.Length * (availableTextWidth_jug / waterJugTextSize.X));
                    textToDraw_jug = textToDraw_jug.Substring(0, Math.Max(0, numChars - 3)) + "...";
                }

                Raylib.DrawTextEx(_mainFont, textToDraw_jug,
                    new Vector2(shopListAreaX + 10, itemEntryY + (SHOP_ITEM_ENTRY_HEIGHT - 20) / 2),
                    20, 1, Color.Blue);

                Rectangle buy1JugButtonRect = new Rectangle(buttonsXStart_jug, itemEntryY + (SHOP_ITEM_ENTRY_HEIGHT - SHOP_BUTTON_HEIGHT) / 2, SHOP_BUTTON_WIDTH, SHOP_BUTTON_HEIGHT);
                Rectangle buy5JugButtonRect = new Rectangle(buttonsXStart_jug + SHOP_BUTTON_WIDTH + 10, itemEntryY + (SHOP_ITEM_ENTRY_HEIGHT - SHOP_BUTTON_HEIGHT) / 2, SHOP_BUTTON_WIDTH, SHOP_BUTTON_HEIGHT);

                DrawTextButton("Buy 1", buy1JugButtonRect, Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), buy1JugButtonRect), 16, Color.Black, Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), buy1JugButtonRect) ? new Color(170, 210, 240, 255) : new Color(130, 180, 230, 255));
                DrawTextButton("Buy 5", buy5JugButtonRect, Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), buy5JugButtonRect), 16, Color.Black, Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), buy5JugButtonRect) ? new Color(170, 210, 240, 255) : new Color(130, 180, 230, 255));
            }

            Raylib.EndScissorMode();

            bool mouseOverClose = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), _shopCloseButtonDest);
            DrawTextButton("Close Shop", _shopCloseButtonDest, mouseOverClose, 20);
        }

        private void UpdateShopScreenInput(GameManager gameManager)
        {
            Vector2 mousePosition = this.GameCursorPosition;

            // --- Scrolling Logic ---
            float totalContentHeight = (gameManager.AllDefinedIngredients.Count + 1) * (SHOP_ITEM_ENTRY_HEIGHT + SHOP_LIST_ITEM_PADDING);
            float shopListVisibleHeight = _screenHeight - SHOP_LIST_START_Y - 90;
            if (totalContentHeight > shopListVisibleHeight)
            {
                Rectangle shopListAreaForScroll = new Rectangle(0, SHOP_LIST_START_Y, _screenWidth, shopListVisibleHeight);
                if (Raylib.CheckCollisionPointRec(mousePosition, shopListAreaForScroll))
                {
                    float wheelMove = Raylib.GetMouseWheelMove();
                    if (wheelMove != 0)
                    {
                        _shopScrollOffset.Y += wheelMove * SHOP_SCROLL_SPEED_MULTIPLIER;
                        _shopScrollOffset.Y = Math.Clamp(_shopScrollOffset.Y, shopListVisibleHeight - totalContentHeight, 0);
                    }
                }
            }
            else
            {
                _shopScrollOffset.Y = 0;
            }

            // --- Button Click Logic ---
            if (Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                // Check the CLOSE button FIRST
                if (Raylib.CheckCollisionPointRec(mousePosition, _shopCloseButtonDest))
                {
                    gameManager.CloseShop();
                    return; 
                }
                
                for (int i = 0; i < _shopIngredientLayout.Count; i++)
                {
                    Ingredient ing = _shopIngredientLayout[i].Item1;
                    if (!gameManager.GloballyUnlockedIngredientTypes.Contains(ing.Type)) continue;

                    Rectangle buy1Rect = _shopIngredientLayout[i].Item2;
                    Rectangle buy10Rect = _shopIngredientLayout[i].Item3;

                    if (Raylib.CheckCollisionPointRec(mousePosition, buy1Rect))
                    {
                        gameManager.PlayerAttemptPurchaseIngredient(ing, 1);
                        return;
                    }
                    if (Raylib.CheckCollisionPointRec(mousePosition, buy10Rect))
                    {
                        gameManager.PlayerAttemptPurchaseIngredient(ing, 10);
                        return;
                    }
                }
            

                // Check Water Jug Buy Buttons
                float itemEntryY_forWaterJug = SHOP_LIST_START_Y + _shopScrollOffset.Y;
                foreach (var ing_item in gameManager.AllDefinedIngredients.OrderBy(ing => ing.Name))
                {
                    itemEntryY_forWaterJug += SHOP_ITEM_ENTRY_HEIGHT + SHOP_LIST_ITEM_PADDING;
                }

                if (itemEntryY_forWaterJug + SHOP_ITEM_ENTRY_HEIGHT >= SHOP_LIST_START_Y &&
                    itemEntryY_forWaterJug <= SHOP_LIST_START_Y + shopListVisibleHeight)
                {
                    float shopListAreaX_input = 50;
                    float shopListWidth_input = _screenWidth - 100;
                    float buttonGroupTotalWidth_input = (SHOP_BUTTON_WIDTH * 2) + 10;
                    float rightPaddingForButtons_input = 10;
                    float buttonsXStart_jug_input = shopListAreaX_input + shopListWidth_input - buttonGroupTotalWidth_input - rightPaddingForButtons_input;

                    Rectangle buy1JugButtonRect_input = new Rectangle(buttonsXStart_jug_input, itemEntryY_forWaterJug + (SHOP_ITEM_ENTRY_HEIGHT - SHOP_BUTTON_HEIGHT) / 2, SHOP_BUTTON_WIDTH, SHOP_BUTTON_HEIGHT);
                    Rectangle buy5JugButtonRect_input = new Rectangle(buttonsXStart_jug_input + SHOP_BUTTON_WIDTH + 10, itemEntryY_forWaterJug + (SHOP_ITEM_ENTRY_HEIGHT - SHOP_BUTTON_HEIGHT) / 2, SHOP_BUTTON_WIDTH, SHOP_BUTTON_HEIGHT);

                    if (Raylib.CheckCollisionPointRec(mousePosition, buy1JugButtonRect_input))
                    {
                        Console.WriteLine("UIManager: Clicked 'Buy 1 Water Jug'");
                        _lastClickProcessedTime = (float)Raylib.GetTime();
                        gameManager.PlayerAttemptPurchaseWaterJug(1);
                        return;
                    }
                    if (Raylib.CheckCollisionPointRec(mousePosition, buy5JugButtonRect_input))
                    {
                        Console.WriteLine("UIManager: Clicked 'Buy 5 Water Jugs'");
                        _lastClickProcessedTime = (float)Raylib.GetTime();
                        gameManager.PlayerAttemptPurchaseWaterJug(5);
                        return;
                    }

                }
            }
        }
            
        public void ClearWorkstationMessage()
        {
            _workstationMessage = string.Empty;
            _workstationMessageTimer = 0f;
        }
    }
}

