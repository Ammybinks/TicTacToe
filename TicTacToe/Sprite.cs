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
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace SpriteLibrary
{
    public class Sprite
    {
        // upper-left coordinate of the sprite image on the screen
        public Vector2 UpperLeft;

        // X and Y stretching factors to adjust the final sprite dimensions
        public Vector2 Scale = new Vector2(1.0f, 1.0f);

        // this member holds the current Texture for the sprite
        private Texture2D spriteTexture;

        // if true, then sprite is visible and can move and interact (collide) with others
        public bool IsAlive = true;

        // fastest absolute speed the sprite will move
        public double MaxSpeed = 10;

        // current sprite velocity (X and Y speed components)
        private Vector2 velocity;

        // current sprite movement direction, in degrees
        // (even if Velocity is 0,0 we  still want to "point" in a direction)
        private double directionAngle = 0;

        // Time-To-Live TimeSpan indicates remaining time sprite will last alive (-1 = disabled)
        public int TTL = -1;

        // current value, in degrees, of the rotation of the sprite
        public double RotationAngle = 0;

        // origin for the sprite (defaults to Upper-Left)
        public Vector2 Origin = new Vector2(0, 0);

        // indication of which depth to use when drawing sprite (for layering purposes)
        public float LayerDepth = 0;

        // color array used internally for collision detection
        private Color[,] textureColors;

        // the desired number of milliseconds between animation frame changes
        // if zero (default), animation will advae onnc each call to animate().
        public int AnimationInterval = 0;

        // the time in milliseconds when the last animation frame was changed
        private int lastAnimationTime = 0;

        // public flag indicating whether or not animation frames should be continuously looped
        public bool ContinuousAnimation = true;

        // if ContinuousAnimation = false, this flag indicates whether or not a 
        // "short" animation sequence is currently active
        private bool animationShortStarted = false;

        // this variable contains the stop frame for a short animation sequence
        private int animationShortStopFrame = 0;

        // this variable contains a single frame that will be displayed after
        // the short animation sequence is complete.
        private int animationShortFinalFrame = 0;
        
        // This internal member tracks the number of frames in the animation strip
        private int numFrames = 1;

        // This internal member represents the current source rectangle in the animation strip
        private Rectangle imageRect;

        // This internal member specifies the width of a single frame in the animation strip
        // (Note:  overall strip width should be an even multiple of this value!)
        private int frameWidth;

        // This internal member shows the current animation frame (should be 0 -> numFrames-1)
        private int currentFrame = 0;

        public int getCurrentFrame()
        {
            return currentFrame;
        }

        public void setCurrentFrame(int frame)
        {
            if (frame > numFrames - 1)  // safety check!
            {
                currentFrame = 0;
                imageRect = new Rectangle(0, 0, frameWidth, spriteTexture.Height);
            }
            else
            {
                currentFrame = frame;
                imageRect = new Rectangle(frameWidth * frame, 0, frameWidth, spriteTexture.Height);
            }
        }

        // This method will load the Texture based on the image name
        public void SetTexture(Texture2D texture)
        {
            SetTexture(texture, 1);
        }


        // This method will load the Texture based on the image name and number of frames
        public void SetTexture(Texture2D texture, int frames)
        {
            numFrames = frames;

            spriteTexture = texture;
            int width = spriteTexture.Width;
            int height = spriteTexture.Height;

            frameWidth = width / numFrames;   // does not include effects of scaling!

            // does not include effects of scaling, which may change after SetTexture is finished!
            imageRect = new Rectangle(0, 0, frameWidth, height);

            // create a color matrix that we'll use later for collision detection
            // contains colors for the entire image (including any animation strip)
            Color[] colorData = new Color[width * height];
            spriteTexture.GetData(colorData);

            textureColors = new Color[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    textureColors[x, y] = colorData[x + y * width];
                }
            }
        }

        // This method will draw the sprite using the current position, rotation, scale, and layer depth
        public virtual void Draw(SpriteBatch theSpriteBatch)
        {
            float radians = MathHelper.ToRadians((float)RotationAngle);
            if (IsAlive)
                theSpriteBatch.Draw(spriteTexture, UpperLeft + Origin, imageRect, Color.White,
                                    -radians, Origin / Scale, Scale, SpriteEffects.None, LayerDepth);
        }

        // This method will draw the sprite using the current position, rotation, scale, and layer depth
        public virtual void Draw(SpriteBatch theSpriteBatch, Vector2 cameraUpperLeft)
        {
            UpperLeft -= cameraUpperLeft;
            Draw(theSpriteBatch);
            UpperLeft += cameraUpperLeft;
        }

        private Matrix getTransformMatrix()
        {
            // see this link for a great description of transformation matrix creation:
            // http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series2D/Coll_Detection_Matrices.php
            return 
                Matrix.CreateTranslation(-Origin.X / Scale.X, -Origin.Y / Scale.Y, 0) *
                Matrix.CreateRotationZ(-MathHelper.ToRadians((float)this.RotationAngle)) *
                Matrix.CreateScale(Scale.X, Scale.Y, 1.0f) *
                Matrix.CreateTranslation(UpperLeft.X + (float)Origin.X, UpperLeft.Y + (float)Origin.Y, 0);
        }

        public virtual void DrawBoundingRectangle(SpriteBatch theSpriteBatch,Texture2D borderTexture)
        {
            if (!IsAlive)
                return;

            // this part will draw the non-rotated bounding rectangle
            Rectangle rect1 = GetBoundingRectangle();
            Rectangle topRect = new Rectangle(rect1.Left, rect1.Top, rect1.Width, 1);
            Rectangle bottomRect = new Rectangle(rect1.Left, rect1.Top + rect1.Height, rect1.Width, 1);
            Rectangle leftRect = new Rectangle(rect1.Left, rect1.Top, 1, rect1.Height);
            Rectangle rightRect = new Rectangle(rect1.Left + rect1.Width, rect1.Top, 1, rect1.Height);

            theSpriteBatch.Draw(borderTexture, topRect, null, Color.White,0, new Vector2(0, 0), SpriteEffects.None, 0);
            theSpriteBatch.Draw(borderTexture, bottomRect, null, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, 0);
            theSpriteBatch.Draw(borderTexture, leftRect, null, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, 0);
            theSpriteBatch.Draw(borderTexture, rightRect, null, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, 0);

            //// this part will draw the rotated bounding rectangle
            //Matrix xformMatrix = getTransformMatrix();

            //int width = textureColors.GetLength(0) / numFrames;
            //int height = textureColors.GetLength(1);
            //for (int x = 0; x < width; x++)
            //{
            //    Vector2 pos = new Vector2(x, 0);
            //    Vector2 pos2 = Vector2.Transform(pos, xformMatrix);
            //    theSpriteBatch.Draw(borderTexture, new Rectangle((int)pos2.X, (int)pos2.Y, 1, 1), null, Color.Yellow, 0, new Vector2(0, 0), SpriteEffects.None, 0);

            //    Vector2 pos3 = new Vector2(x, height - 1);
            //    Vector2 pos4 = Vector2.Transform(pos3, xformMatrix);
            //    theSpriteBatch.Draw(borderTexture, new Rectangle((int)pos4.X, (int)pos4.Y, 1, 1), null, Color.Yellow, 0, new Vector2(0, 0), SpriteEffects.None, 0);
            //}

            //for (int y = 0; y < height; y++)
            //{
            //    Vector2 pos = new Vector2(0, y);
            //    Vector2 pos2 = Vector2.Transform(pos, xformMatrix);
            //    theSpriteBatch.Draw(borderTexture, new Rectangle((int)pos2.X, (int)pos2.Y, 1, 1), null, Color.Yellow, 0, new Vector2(0, 0), SpriteEffects.None, 0);

            //    Vector2 pos3 = new Vector2(width - 1, y);
            //    Vector2 pos4 = Vector2.Transform(pos3, xformMatrix);
            //    theSpriteBatch.Draw(borderTexture, new Rectangle((int)pos4.X, (int)pos4.Y, 1, 1), null, Color.Yellow, 0, new Vector2(0, 0), SpriteEffects.None, 0);
            //}
        }

        // calculate final sprite width, accounting for scale and assuming zero rotation
        public int GetWidth()
        {
            return (int)((float)spriteTexture.Width * Scale.X / (float)numFrames);
        }

        // calculate final sprite height, accounting for scale and assuming zero rotation
        public int GetHeight()
        {
            return (int)((float)spriteTexture.Height * Scale.Y);
        }

        // calculate current center offset from the UpperLeft, accounting for scale and assuming zero rotation
        public Vector2 GetCenter()
        {
            return new Vector2(GetWidth() / 2, GetHeight() / 2);
        }

        public Vector2 GetVelocity()
        {
            return velocity;
        }

        // Set the velocity based on the specified absolute speed and direction angle
        public void SetSpeedAndDirection(double speed, double angle)
        {
            directionAngle = angle;
            double radians = MathHelper.ToRadians((float)directionAngle);   // convert current sprite angle to radians

            // calculate the X and Y velocity components based on the current angle and input speed
            double VX = speed * Math.Cos(radians);
            double VY = -speed * Math.Sin(radians); // -1 corrects for computer's "up"

            SetVelocity(VX, VY);
            directionAngle = angle; // just in case speed is zero, we still want to update the direction!
        }

        public double GetDirectionAngle()
        {
            return directionAngle;
        }

        static public double CalculateDirectionAngle(Vector2 vect)
        {
            double angle = 0.0f;
            double currentRadians = Math.Atan2(-vect.Y ,  vect.X);
            angle = MathHelper.ToDegrees((float)currentRadians);

            if (angle < 0)
                angle += 360;
            return angle;

        }

        public void SetDirectionAngle(double newAngle)
        {
            double currentSpeed = Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y);
            SetSpeedAndDirection(currentSpeed, newAngle);
        }

        public void ChangeDirectionAngle(double directionAngleDelta)
        {
            directionAngle += directionAngleDelta;
            if (directionAngle < 0)
                directionAngle += 360;
            if (directionAngle >= 360)
                directionAngle -= 360;

            double currentSpeed = Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y);
            SetSpeedAndDirection(currentSpeed, directionAngle);
        }

        // Set the velocity to the specified X and Y components, subject to configured max speed
        public void SetVelocity(double velocityX, double velocityY)
        {
            velocity.X = (float)velocityX; //update the X speed component
            velocity.Y = (float)velocityY; //update the Y speed component

            // calculate overall speed using Pythagorean's theorem:
            // s*s = x*x + y*y, or s = sqrt(x*x + y*y)

            double newSpeed = Math.Sqrt(velocityX * velocityX + velocityY * velocityY);

            // make sure we don't get going too fast.  
            if (newSpeed > MaxSpeed) //if new speed exceeds maximum
            {
                // we want to scale back both the X and Y speed components such that they stay within the max
                double reductionFactor = MaxSpeed / newSpeed;

                velocity.X *= (float)reductionFactor;    // reduce X component
                velocity.Y *= (float)reductionFactor;    // reduce Y component
            }

            directionAngle = CalculateDirectionAngle(velocity);  // update new directionAngle from velocity
        }

        // this function will move the sprite according to it's current Velocity
        public bool Move()
        {
            if (!checkTTL())
                return false;

            // create and calculate the new position based on current upper-left coordinate and velocity
            Vector2 newPosition;

            newPosition.X = UpperLeft.X + velocity.X;
            newPosition.Y = UpperLeft.Y + velocity.Y;

            // update the sprite's current UpperLeft coordinates with the final position
            UpperLeft = newPosition;

            return true;
        }

        // this method will move the sprite and wrap it around to the other side when it goes off the screen
        public void MoveAndWrap(float screenSizeX, float screenSizeY)
        {
            if (!Move())
                return;

            int width = GetWidth(); // takes into account scale
            int height = GetHeight();

            // wrap left - if sprite is all the way off the left side, 
            // place it as just coming in from the right side
            if (UpperLeft.X < (0 - width))
                UpperLeft.X = screenSizeX;

            // wrap right - if sprite is all the way off the right side, 
            // place it as just coming in from the left side
            if (UpperLeft.X > (screenSizeX))
                UpperLeft.X = 0 - width;

            // wrap top - if sprite is all the way off the top side, 
            // place it as just coming in from the bottom side
            if (UpperLeft.Y < (-height))
                UpperLeft.Y = screenSizeY;

            // wrap bottom - if sprite is all the way off the bottom side, 
            // place it as just coming in from the top side
            if (UpperLeft.Y > (screenSizeY))
                UpperLeft.Y = 0 - height;
        }

        // this method will move the sprite until it reaches a screen edge, where it will become dead
        public void MoveAndVanish(float screenSizeX, float screenSizeY)
        {
            if (!Move())
                return;

            int width = GetWidth(); // takes into account scale
            int height = GetHeight();

            // if sprite is all the way off the left side, 
            if (UpperLeft.X < (0 - width))
            {
                IsAlive = false;
                return;
            }

            // if sprite is all the way off the right side, 
            if (UpperLeft.X > (screenSizeX))
            {
                IsAlive = false;
                return;
            }

            // if sprite is all the way off the top side, 
            if (UpperLeft.Y < (-height))
            {
                IsAlive = false;
                return;
            }

            // if sprite is all the way off the bottom side, 
            if (UpperLeft.Y > (screenSizeY))
            {
                IsAlive = false;
                return;
            }
        }

        /// ///

        // adjust the current rotation angle by the indicated amount (in degrees)
        public void ChangeRotationAngle(double delta)
        {
            // adjust the sprite's current angle by the input amount (may be positive or negative)
            RotationAngle += delta;

            // keep the sprite's angle between [0,359] so it's easy to read and understand
            if (RotationAngle < 0.0)
                RotationAngle += 360.0;

            if (RotationAngle >= 360.0)
                RotationAngle -= 360.0;
        }

        // accelerate the sprite (adjust its velocity) by the specified absolute amount
        public void Accelerate(double acceleration)
        {
            double radians = MathHelper.ToRadians((float)directionAngle);   // convert current sprite angle to radians

            // calculate acceleration components
            double AX = acceleration * Math.Cos(radians);
            double AY = acceleration * Math.Sin(radians);

            // negative AY to account for computer's "up"
            Accelerate(AX, -AY);  // now accelerate based on individual X and Y components
        }

        // accelerate the sprite (adjust its velocity) by the specified X and Y components
        public void Accelerate(double AX, double AY)
        {
            // calculate new X and Y velocity components
            double speedX = velocity.X + AX;
            double speedY = velocity.Y + AY; 

            // update the speed, ensuring it does not exceed the maximum
            SetVelocity(speedX, speedY);
        }

        public void Reflect(Vector2 slope)
        {
            // make a copy of the input because we'll modfiy the value
            Vector2 slopeNormal = new Vector2(slope.X,slope.Y);
            slopeNormal.Normalize();    // make unit length

            // cross product of normal and (0,0,-1) to get normal to surface
            Vector3 normal3D = Vector3.Cross(new Vector3(slopeNormal, 0), new Vector3(0, 0, -1));

            // get the X and Y components only from the 3D vector
            Vector2 normal = new Vector2(normal3D.X, normal3D.Y);

            // now reflect the sprite's current velocity against the normal
            velocity = Vector2.Reflect(velocity, normal);

            directionAngle = CalculateDirectionAngle(velocity);  // update new directionAngle from velocity

        }

        private bool checkTTL()
        {
            // If Time-to-live is configured, reduce it by one
            if (TTL >= 1)
                TTL -= 1;

            else if (TTL == 0)   // if Time-to-Live has been reduced to zero, turn off sprite
                IsAlive = false;

            return IsAlive; // return true if sprite is still alive
        }


        public Rectangle GetBoundingRectangle()
        {
            if (RotationAngle == 0)
            {
                // when there is no rotation, return simple rectangle without resorting to complex math!
                // construct a new rectangle based on the UpperLeft point and current scaled width, height
                return new Rectangle((int)UpperLeft.X, (int)UpperLeft.Y, GetWidth(), GetHeight());
            }
            else
            {
                // this sprite is currently rotated, so we need to compute new UpperLeft and LowerRight
                // points based on the current rotation and scaling, then form bounding rectangle from those points.
                Matrix xformMatrix = getTransformMatrix();

                int width = textureColors.GetLength(0) / numFrames;
                int height = textureColors.GetLength(1);
                Vector2 actualUpperLeft = Vector2.Transform(new Vector2(0, 0), xformMatrix);
                Vector2 actualUpperRight = Vector2.Transform(new Vector2(width, 0), xformMatrix);
                Vector2 actualLowerLeft = Vector2.Transform(new Vector2(0, height), xformMatrix);
                Vector2 actualLowerRight = Vector2.Transform(new Vector2(width, height), xformMatrix);

                int minX = Math.Min((int)actualUpperLeft.X, (int)actualUpperRight.X);
                minX = Math.Min(minX, (int)actualLowerLeft.X);
                minX = Math.Min(minX, (int)actualLowerRight.X);

                int minY = Math.Min((int)actualUpperLeft.Y, (int)actualUpperRight.Y);
                minY = Math.Min(minY, (int)actualLowerLeft.Y);
                minY = Math.Min(minY, (int)actualLowerRight.Y);

                int maxX = Math.Max((int)actualUpperLeft.X, (int)actualUpperRight.X);
                maxX = Math.Max(maxX, (int)actualLowerLeft.X);
                maxX = Math.Max(maxX, (int)actualLowerRight.X);

                int maxY = Math.Max((int)actualUpperLeft.Y, (int)actualUpperRight.Y);
                maxY = Math.Max(maxY, (int)actualLowerLeft.Y);
                maxY = Math.Max(maxY, (int)actualLowerRight.Y);

                return new Rectangle(minX, minY, maxX - minX, maxY - minY);
            }
        }

        // returns true if this Sprite's bounding rectangle intesects with the target sprite's rectangle.
        // (assuming both Sprites are alive!)
        public bool IsCollided(Sprite otherSprite)
        {
            if (!IsAlive)
                return false;

            if (!otherSprite.IsAlive)
                return false;

            Rectangle thisRect = GetBoundingRectangle();
            Rectangle otherRect = otherSprite.GetBoundingRectangle();

            // if the bounding rectangles of the sprite intersect
            if (thisRect.Intersects(otherRect))
            {
                // see if individual pixels have collided
                return IsCollidedPixels(otherSprite);
            }

            // otherwise, no collision
            return false;
        }


        private bool IsCollidedPixels(Sprite otherSprite)
        {
            // to account for rotation, scaling, and positioning -- get the transform matrix for both sprites!
            // see http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series2D/Coll_Detection_Overview.php

            Matrix thisMatrix = getTransformMatrix();
            Matrix otherMatrix = otherSprite.getTransformMatrix();
            
            // create a matrix that will translate directly from sprite 1 coordinates to sprite 2 coordinates
            Matrix thisToOtherMatrix = thisMatrix * Matrix.Invert(otherMatrix);

            int thisWidth = textureColors.GetLength(0) / numFrames;
            int thisHeight = textureColors.GetLength(1);
            int thisFrame = currentFrame;

            int otherWidth = otherSprite.textureColors.GetLength(0) / otherSprite.numFrames;
            int otherHeight = otherSprite.textureColors.GetLength(1);
            int otherFrame = otherSprite.getCurrentFrame();

            for (int x = 0; x < thisWidth; x++)
            {
                for (int y = 0; y < thisHeight; y++)
                {
                    Vector2 thisPos = new Vector2(x, y);
                    Vector2 otherPos = Vector2.Transform(thisPos, thisToOtherMatrix);

                    int otherX = (int)otherPos.X;
                    int otherY = (int)otherPos.Y;

                    if ((otherX >= 0) && (otherX < otherWidth))
                    {
                        if ((otherY >= 0) && (otherY < otherHeight))
                        {
                            //int textureX = (int)(((float)(x + thisFrame * thisWidth)) / Scale.X);
                            //int textureY = (int)((float)y / Scale.Y);
                            //if (textureColors[textureX, textureY].A > 0)
                            if (textureColors[x, y].A > 0)
                            {
                                //int otherTextureX = (int)(((float)(otherX + otherFrame * otherWidth)) / otherSprite.Scale.X);
                                //int otherTextureY = (int)((float)otherY / otherSprite.Scale.Y);
                                if (otherSprite.textureColors[otherX, otherY].A > 0)
                                {
                                    return true;    // collision!
                                }
                            }
                        }
                    }
                }
            }

            return false;   // no collision
        }

        // this public method will launch a "short" animation sequence starting 
        // at the specified frame and stopping at the specified frame.  After the
        // animation ends the image will revert to the static "final" frame
        public void StartAnimationShort(int startFrame, int stopFrame, int finalFrame)
        {
            // set starting frame as current frame
            currentFrame = startFrame;

            // store other input variables
            animationShortStopFrame = stopFrame;
            animationShortFinalFrame = finalFrame;

            // launch the short animation!
            animationShortStarted = true;
        }

        // this public method will return true if the image is animating either
        // continuously or is amidst a "short" animation sequence
        public bool IsAnimating()
        {
            return (animationShortStarted || ContinuousAnimation);
        }

        // games will call this public method to allow the sprite to advance to
        // the next animation frame if required, based on the pre-configured
        // animation interval and current animation state.
        public void Animate(GameTime gameTime)
        {
            // if we are currently supposed to be animating for any reason
            if (IsAnimating())
            {
                // advance to the next frame if enough game time has elapsed
                advanceFrame(gameTime);
            }
        }


        // change the animation to the next frame 
        private void advanceFrame(GameTime gameTime)
        {
            // get the current time
            int now = (int)gameTime.TotalGameTime.TotalMilliseconds;

            // if we have not yet reached our next scheduled frame change
            if (now < (lastAnimationTime + AnimationInterval))
            {
                return; // not time to advance frame yet
            }

            // figure out our last frame based on continuous or "short" mode
            int endFrame = numFrames - 1;   // default for continuous animation
            if (animationShortStarted)
                endFrame = animationShortStopFrame;

            // if we are not yet done with this sequence
            if (currentFrame < endFrame)
            {
                currentFrame += 1;  // move to the next frame
            }
            else
            {
                // if continuous animation, reset sequence to 0
                if (ContinuousAnimation)
                {
                    currentFrame = 0;
                }
                else
                {
                    // for animation short, set current frame to final frame
                    currentFrame = animationShortFinalFrame;
                    animationShortStarted = false;  // no longer animating
                }
            }
            // adjust imageRec.X to match new current frame
            imageRect.X = currentFrame * frameWidth;

            // update last animation time
            lastAnimationTime = (int)gameTime.TotalGameTime.TotalMilliseconds;
        }

    }
}
