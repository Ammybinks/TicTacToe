////////////////////////////////////////////////////////////////
// Copyright 2013, CompuScholar, Inc.
//
// This source code is for use by the students and teachers who 
// have purchased the corresponding TeenCoder or KidCoder product.
// It may not be transmitted to other parties for any reason
// without the written consent of CompuScholar, Inc.
// This source is provided as-is for educational purposes only.
// CompuScholar, Inc. makes no warranty and assumes
// no liability regarding the functionality of this program.
//
////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using SpriteLibrary;

namespace TicTacToe
{
    // The Marker class represents a normal Sprite
    // plus an indication of which player placed
    // the X or O symbol
    public class Marker : Sprite
    {
        public int Player;
    };

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class TicTacToe : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // these textures represent all of the images used in the game
        Texture2D oTexture;
        Texture2D xTexture;
        Texture2D boardTexture;
        Texture2D splashTexture;

        // these sprites are used to display the overlay and splash images
        Sprite overlay;
        Sprite splash;

        // the gameboard is a 2D (3x3) grid of Marker objects
        public Marker[,] gameboard;

        // these properties keep track of the player scores and current turn
        public int player1Score;
        public int player2Score;
        public int currentPlayer;

        // this font is used to display all text in the game
        SpriteFont gameFont;

        // constants to help define the grid size and display dimensions
        private const int GRID_SIZE = 3;
        private const int CELL_SIZE = 50;
        private const int LINE_WIDTH = 25;

        // this enumeration represnts all game screens
        enum GameScreen
        {
            Splash,
            Menu,
            Playing,
            GameOver
        };

        // here we keep track of the current screen
        GameScreen currentScreen;

        // user input handling variables
        KeyboardState oldKeyboardState;
        MouseState oldMouseState;

        // display the winning game message
        String gameOverMessage;

        // This method is provided complete as part of the activity starter.
        public TicTacToe()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        // This method is provided complete as part of the activity starter.
        protected override void Initialize()
        {
            // call base initialize first to run LoadContent and get all textures loaded
            // prior to initializing sprites
            base.Initialize();

            // start with the splash screen
            currentScreen = GameScreen.Splash;

            // set the screen size smaller than normal; don't need a huge window!
            this.graphics.PreferredBackBufferWidth = 400;
            this.graphics.PreferredBackBufferHeight = 400;
            this.graphics.ApplyChanges();

            // create a sprite for the splash screen image
            splash = new Sprite();
            splash.SetTexture(splashTexture);

            // center the splash on the screen
            splash.UpperLeft = new Vector2(this.GraphicsDevice.Viewport.Width / 2 - splash.GetWidth() / 2,
                                           this.GraphicsDevice.Viewport.Height / 2 - splash.GetHeight() / 2);

            // create a sprite for the game board overlay
            overlay = new Sprite();
            overlay.SetTexture(boardTexture);

            // center the board on the screen
            overlay.UpperLeft = new Vector2(this.GraphicsDevice.Viewport.Width / 2 - overlay.GetWidth() / 2,
                                            this.GraphicsDevice.Viewport.Height / 2 - overlay.GetHeight() / 2);


        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        // This method is provided complete as part of the activity starter.
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // load the game font to be used for all text
            gameFont = this.Content.Load<SpriteFont>("GameFont");

            // load all textures to be used in the game
            oTexture = this.Content.Load<Texture2D>("Images\\oSymbol");
            xTexture = this.Content.Load<Texture2D>("Images\\xSymbol");
            boardTexture = this.Content.Load<Texture2D>("Images\\tic_tac_toe_board2");
            splashTexture = this.Content.Load<Texture2D>("Images\\tic_tac_toe_splash");

        }

        // This method is provided complete as part of the activity starter.
        private void startGame()
        {
            // when a new game is started...

            // create new 2D array of markers for the game board 
            // (all elements are initially NULL)
            gameboard = new Marker[GRID_SIZE, GRID_SIZE];

            // we are now on the playing screen with the X player to start
            currentScreen = GameScreen.Playing;
            currentPlayer = 1;

            // let's see the mouse cursor for this screen
            this.IsMouseVisible = true;
        }

        // This method is provided complete as part of the activity starter.
        private void stopGame(String message)
        {
            // to stop the game, save the game-over message
            gameOverMessage = message;
            // move to the game over screen
            currentScreen = GameScreen.GameOver;
            // hide the mouse
            this.IsMouseVisible = false;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        // This method is provided complete as part of the activity starter.
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        // This method is provided complete as part of the activity starter.
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // user input variable initialization
            KeyboardState currentKeyboard = Keyboard.GetState();
            if (oldKeyboardState == null)
                oldKeyboardState = currentKeyboard;

            // pick the right update method to call based on the current screen
            if (currentScreen == GameScreen.Splash)
            {
                updateSplash(gameTime, currentKeyboard);
            }
            else if (currentScreen == GameScreen.Menu)
            {
                updateMenu(gameTime, currentKeyboard);
            }
            else if (currentScreen == GameScreen.Playing)
            {
                updatePlaying(gameTime, currentKeyboard);
            }
            else if (currentScreen == GameScreen.GameOver)
            {
                updateGameover(gameTime, currentKeyboard);
            }

            // save old keyboard state
            oldKeyboardState = currentKeyboard;

            base.Update(gameTime);
        }

        // This method is provided complete as part of the activity starter.
        private void updateSplash(GameTime gameTime, KeyboardState currentKeyboard)
        {
            // keep the splash displayed for a few seconds
            if (gameTime.TotalGameTime > TimeSpan.FromSeconds(5))
            {
                // move to menu screen
                currentScreen = GameScreen.Menu;
            }
        }

        // This method is provided complete as part of the activity starter.
        private bool wasKeyPressed(Keys target, KeyboardState currentKeyboard)
        {
            // return true if the target key has just been pressed
            if (currentKeyboard.IsKeyUp(target) && oldKeyboardState.IsKeyDown(target))
                return true;
            else
                return false;
        }

        // This method is provided complete as part of the activity starter.
        private void updateMenu(GameTime gameTime, KeyboardState currentKeyboard)
        {
            // from the menu, a space bar press will start a new round of games
            if (wasKeyPressed(Keys.Space, currentKeyboard))
            {
                player1Score = 0;
                player2Score = 0;
                startGame();
            }
            else if (wasKeyPressed(Keys.Escape, currentKeyboard))
            {
                // the escape key will exit the program
                Exit();
            }

        }

        // This method is provided complete as part of the activity starter.
        private bool wasLeftMouseButtonPressed(MouseState currentMouse)
        {
            // return true if the left mouse button is now pressed
            if ((currentMouse.LeftButton == ButtonState.Released) && (oldMouseState.LeftButton == ButtonState.Pressed))
                return true;
            else
                return false;
        }

        // This method is provided complete as part of the activity starter.
        private void updatePlaying(GameTime gameTime, KeyboardState currentKeyboard)
        {
            // when playing the game, need to track the mouse state
            MouseState currentMouse = Mouse.GetState();
            if (oldMouseState == null)
                oldMouseState = currentMouse;

            // when the user left-clicks the mouse
            if (wasLeftMouseButtonPressed(currentMouse))
            {
                // get mouse click position relative to UpperLeft point on board
                Vector2 clickPosition = new Vector2(currentMouse.X, currentMouse.Y) - overlay.UpperLeft;

                // assume the mouse click was not on a valid column and row
                int col = -1;
                int row = -1;

                // now check each row and column (0-2) at the same time
                for (int i = 0; i < GRID_SIZE; i++)
                {
                    // calculate the min and max coordinate for this cell 
                    // (both horizontal and vertical since it's a square!)
                    int bound1 = i * (CELL_SIZE + LINE_WIDTH);
                    int bound2 = bound1 + CELL_SIZE;

                    // see if the X-position was within this cell boundary
                    if ((clickPosition.X >= bound1) && (clickPosition.X < bound2))
                    {
                        col = i;    // yes user clicked on a valid column
                    }
                    // see if the Y-position was within this cell boundary
                    if ((clickPosition.Y >= bound1) && (clickPosition.Y < bound2))
                    {
                        row = i;    // yes user clicked on a valid row
                    }
                }

                // if user clicked on a valid row and column
                if ((col != -1) && (row != -1))
                {
                    // if the target cell is currently null (no marker)
                    if (gameboard[col, row] == null)
                    {
                        // create a new marker, store the current player number
                        Marker marker = new Marker();
                        marker.Player = currentPlayer;

                        // set the marker texter to X or O based on current player
                        if (currentPlayer == 1)
                            marker.SetTexture(xTexture);
                        else
                            marker.SetTexture(oTexture);

                        // position the marker on the selected square
                        marker.UpperLeft = new Vector2(col * (CELL_SIZE + LINE_WIDTH),
                                                       row * (CELL_SIZE + LINE_WIDTH)) + overlay.UpperLeft;

                        // store the marker in the 2D array
                        gameboard[col, row] = marker;

                        // change players now that a valid move has been made
                        if (currentPlayer == 1)
                            currentPlayer = 2;
                        else
                            currentPlayer = 1;
                    }
                }

                // see if there is now a winner
                int winner = checkForWinner();

                // if player 1 has won
                if (winner == 1)
                {
                    player1Score++;
                    stopGame("Player 1 Wins!");
                }
                else if (winner == 2)   // if player 2 has won
                {
                    player2Score++;
                    stopGame("Player 2 Wins!");
                }
                else if (winner == 3)   // if no player won
                {
                    stopGame("Tie game!");
                }

            }

            // save old mouse state for next time
            oldMouseState = currentMouse;

        }

        // This method is provided complete as part of the activity starter.
        private int checkLine(Marker m1, Marker m2, Marker m3)
        {
            if ((m1 == null) || (m2 == null) || (m3 == null))
                return 0;   // no winner if any marker is null

            if ((m1.Player == 1) && (m2.Player == 1) && (m3.Player == 1))
                return 1;   // player 1 has won!

            if ((m1.Player == 2) && (m2.Player == 2) && (m3.Player == 2))
                return 2;   // player 2 has won!

            return 0; // no player has won yet
        }

        // This method is provided complete as part of the activity starter.
        private int checkForWinner()
        {
            int winner;

            // check horizontal lines
            winner = checkLine(gameboard[0, 0], gameboard[1, 0], gameboard[2, 0]);
            if (winner != 0) return winner; // someone has won!
            winner = checkLine(gameboard[0, 1], gameboard[1, 1], gameboard[2, 1]);
            if (winner != 0) return winner; // someone has won!
            winner = checkLine(gameboard[0, 2], gameboard[1, 2], gameboard[2, 2]);
            if (winner != 0) return winner; // someone has won!


            // check vertical lines
            winner = checkLine(gameboard[0, 0], gameboard[0, 1], gameboard[0, 2]);
            if (winner != 0) return winner; // someone has won!
            winner = checkLine(gameboard[1, 0], gameboard[1, 1], gameboard[1, 2]);
            if (winner != 0) return winner; // someone has won!
            winner = checkLine(gameboard[2, 0], gameboard[2, 1], gameboard[2, 2]);
            if (winner != 0) return winner; // someone has won!

            // check diagonal lines
            winner = checkLine(gameboard[0, 0], gameboard[1, 1], gameboard[2, 2]);
            if (winner != 0) return winner; // someone has won!
            winner = checkLine(gameboard[2, 0], gameboard[1, 1], gameboard[0, 2]);
            if (winner != 0) return winner; // someone has won!

            // check for tie game (all squares filled)
            for (int row = 0; row < GRID_SIZE; row++)
            {
                for (int col = 0; col < GRID_SIZE; col++)
                {
                    if (gameboard[col, row] == null)
                        return 0;   // not a tie game yet
                }
            }

            return 3;   // tie game!
        }

        // This method is provided complete as part of the activity starter.
        private void updateGameover(GameTime gameTime, KeyboardState currentKeyboard)
        {
            // if space bar was pressed
            if (wasKeyPressed(Keys.Space, currentKeyboard))
            {
                // start a new game
                startGame();
            }
            // if escape was pressed
            else if (wasKeyPressed(Keys.Escape, currentKeyboard))
            {
                // return to main menu
                currentScreen = GameScreen.Menu;
            }
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        // The student will complete this method as part of the chapter activity.
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Azure);
            spriteBatch.Begin();

            if (currentScreen == GameScreen.Splash)
                drawSplash(gameTime);

            if (currentScreen == GameScreen.Menu)
                drawMenu(gameTime);

            if (currentScreen == GameScreen.Playing)
                drawPlaying(gameTime);

            if (currentScreen == GameScreen.GameOver)
                drawGameOver(gameTime);


            base.Draw(gameTime);

            spriteBatch.End();
        }

        // The student will complete this method as part of the chapter activity.
        private void drawSplash(GameTime gameTime)
        {
            splash.SetTexture(splashTexture);

            splash.Draw(spriteBatch);
        }

        // The student will complete this method as part of the chapter activity.
        private void drawMenu(GameTime gameTime)
        {
            overlay.Draw(spriteBatch);

            spriteBatch.DrawString(gameFont, "Press SPACE to begin a new round.", new Vector2(5, 5), Color.Black);
            spriteBatch.DrawString(gameFont, "Or ESC to leave,", new Vector2(5, 25), Color.Black);
            spriteBatch.DrawString(gameFont, "but you wouldn't want to do that!",new Vector2(5,45),Color.Black);
        }

        // The student will complete this method as part of the chapter activity.
        private void drawPlaying(GameTime gameTime)
        {
            foreach (Sprite Marker in gameboard)
            {
                if (Marker != null)
                {
                    Marker.Draw(spriteBatch);
                }

                overlay.Draw(spriteBatch);


                spriteBatch.DrawString(gameFont, "Player 1:", new Vector2(5, 350), Color.Black);
                spriteBatch.DrawString(gameFont, "Player 2:", new Vector2(5, 370), Color.Black);

                spriteBatch.DrawString(gameFont, player1Score.ToString(), new Vector2(85, 350), Color.Black);
                spriteBatch.DrawString(gameFont, player2Score.ToString(), new Vector2(85, 370), Color.Black);
            }
        }

        // The student will complete this method as part of the chapter activity.
        private void drawGameOver(GameTime gameTime)
        {
            spriteBatch.DrawString(gameFont, gameOverMessage, new Vector2(5, 5), Color.Black);
            spriteBatch.DrawString(gameFont, "Press SPACE to play again.", new Vector2(5, 25), Color.Black);
            spriteBatch.DrawString(gameFont, "Or ESC to exit to the main menu,", new Vector2(5, 45), Color.Black);
            spriteBatch.DrawString(gameFont, "but you wouldn't want to do that!", new Vector2(5, 65), Color.Black);
        }
    }
}
