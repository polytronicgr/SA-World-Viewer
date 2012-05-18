﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using GTAMapViewer.DFF;

namespace GTAMapViewer.Graphics
{
    internal class ModelShader : ShaderProgram
    {
        private Model myCurrentModel;

        private Matrix4 myViewMatrix;
        private int myViewMatrixLoc;

        private Vector3 myCameraPosition;
        private Vector2 myCameraRotation;
        private Matrix4 myPerspectiveMatrix;

        private bool myPerspectiveChanged;
        private bool myViewChanged;

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public Vector3 CameraPosition
        {
            get
            {
                return myCameraPosition;
            }
            set
            {
                myCameraPosition = value;
                myViewChanged = true;
            }
        }
        public Vector2 CameraRotation
        {
            get { return myCameraRotation; }
            set
            {
                myCameraRotation = value;
                myViewChanged = true;
            }
        }
        public Matrix4 PerspectiveMatrix
        {
            get { return myPerspectiveMatrix; }
            set
            {
                myPerspectiveMatrix = value;
                myPerspectiveChanged = true;
            }
        }

        public ModelShader()
        {
            ShaderBuilder vert = new ShaderBuilder( ShaderType.VertexShader, false );
            vert.AddUniform( ShaderVarType.Mat4, "view_matrix" );
            vert.AddAttribute( ShaderVarType.Vec3, "in_position" );
            vert.AddAttribute( ShaderVarType.Vec2, "in_texcoord" );
            vert.AddVarying( ShaderVarType.Vec3, "var_color" );
            vert.Logic = @"
                void main( void )
                {
                    gl_Position = view_matrix * vec4( in_position, 1 );
                    var_color = vec3( in_texcoord, 1.0 );
                }
            ";

            ShaderBuilder frag = new ShaderBuilder( ShaderType.FragmentShader, false );
            frag.AddVarying( ShaderVarType.Vec3, "var_color" );
            frag.Logic = @"
                void main( void )
                {
                    out_frag_colour = vec4( var_color, 1.0 );
                }
            ";

            VertexSource = vert.Generate( GL3 );
            FragmentSource = frag.Generate( GL3 );

            BeginMode = BeginMode.Triangles;

            myCameraPosition = new Vector3();
            myCameraRotation = new Vector2( MathHelper.Pi * 30.0f / 180.0f, 0.0f );

            myPerspectiveChanged = true;
            myViewChanged = true;
        }

        public ModelShader( int width, int height )
            : this()
        {
            Create();
            SetScreenSize( width, height );
        }

        public void SetScreenSize( int width, int height )
        {
            ScreenWidth = width;
            ScreenHeight = height;
            UpdatePerspectiveMatrix();
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddAttribute( "in_position", 3 );
            AddAttribute( "in_texcoord", 2 );

            myViewMatrixLoc = GL.GetUniformLocation( Program, "view_matrix" );
        }

        private void UpdatePerspectiveMatrix()
        {
            PerspectiveMatrix = Matrix4.CreatePerspectiveFieldOfView( (float) Math.PI * ( 60.0f / 180.0f ),
                (float) ScreenWidth / (float) ScreenHeight, 0.125f, 256.0f );
            UpdateViewMatrix();

            myPerspectiveChanged = false;
        }

        private void UpdateViewMatrix()
        {
            Matrix4 yRot = Matrix4.CreateRotationY( myCameraRotation.Y );
            Matrix4 xRot = Matrix4.CreateRotationX( myCameraRotation.X );
            Matrix4 trns = Matrix4.CreateTranslation( -myCameraPosition );

            myViewMatrix = Matrix4.Mult( Matrix4.Mult( Matrix4.Mult( trns, yRot ), xRot ), myPerspectiveMatrix );
            GL.UniformMatrix4( myViewMatrixLoc, false, ref myViewMatrix );

            myViewChanged = false;
        }

        protected override void OnStartBatch()
        {
            if ( myPerspectiveChanged )
                UpdatePerspectiveMatrix();
            else if ( myViewChanged )
                UpdateViewMatrix();

            GL.Enable( EnableCap.DepthTest );
            GL.Enable( EnableCap.CullFace );

            GL.CullFace( CullFaceMode.Front );
            GL.BlendFunc( BlendingFactorSrc.One, BlendingFactorDest.Zero );

            myCurrentModel = null;
        }

        public void Render( Model model )
        {
            if ( model != myCurrentModel )
            {
                if ( myCurrentModel != null )
                    myCurrentModel.VertexBuffer.EndBatch( this );
                model.VertexBuffer.StartBatch( this );
                myCurrentModel = model;
            }

            model.VertexBuffer.Render( this );
        }

        protected override void OnEndBatch()
        {
            if ( myCurrentModel != null )
                myCurrentModel.VertexBuffer.EndBatch( this );

            GL.Disable( EnableCap.DepthTest );
            GL.Disable( EnableCap.CullFace );
        }
    }
}