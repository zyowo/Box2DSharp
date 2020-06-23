using System;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Common;
using Box2DSharp.Dynamics;
using Box2DSharp.Dynamics.Joints;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Common.Input;
using Testbed.Basics;
using Vector2 = System.Numerics.Vector2;

namespace Testbed.Tests
{
    [TestCase("Examples", "Car")]
    public class Car : Test
    {
        private Body _car;

        private float _hz;

        private float _speed;

        private WheelJoint _spring1;

        private WheelJoint _spring2;

        private Body _wheel1;

        private Body _wheel2;

        private float _zeta;

        public Car()
        {
            _hz = 4.0f;
            _zeta = 0.7f;
            _speed = 50.0f;

            Body ground;
            {
                var bd = new BodyDef();
                ground = World.CreateBody(bd);

                var shape = new EdgeShape();

                var fd = new FixtureDef();
                fd.Shape = shape;
                fd.Density = 0.0f;
                fd.Friction = 0.6f;

                shape.Set(new Vector2(-20.0f, 0.0f), new Vector2(20.0f, 0.0f));
                ground.CreateFixture(fd);

                float[] hs = {0.25f, 1.0f, 4.0f, 0.0f, 0.0f, -1.0f, -2.0f, -2.0f, -1.25f, 0.0f};

                float x = 20.0f, y1 = 0.0f, dx = 5.0f;

                for (var i = 0; i < 10; ++i)
                {
                    var y2 = hs[i];
                    shape.Set(new Vector2(x, y1), new Vector2(x + dx, y2));
                    ground.CreateFixture(fd);
                    y1 = y2;
                    x += dx;
                }

                for (var i = 0; i < 10; ++i)
                {
                    var y2 = hs[i];
                    shape.Set(new Vector2(x, y1), new Vector2(x + dx, y2));
                    ground.CreateFixture(fd);
                    y1 = y2;
                    x += dx;
                }

                shape.Set(new Vector2(x, 0.0f), new Vector2(x + 40.0f, 0.0f));
                ground.CreateFixture(fd);

                x += 80.0f;
                shape.Set(new Vector2(x, 0.0f), new Vector2(x + 40.0f, 0.0f));
                ground.CreateFixture(fd);

                x += 40.0f;
                shape.Set(new Vector2(x, 0.0f), new Vector2(x + 10.0f, 5.0f));
                ground.CreateFixture(fd);

                x += 20.0f;
                shape.Set(new Vector2(x, 0.0f), new Vector2(x + 40.0f, 0.0f));
                ground.CreateFixture(fd);

                x += 40.0f;
                shape.Set(new Vector2(x, 0.0f), new Vector2(x, 20.0f));
                ground.CreateFixture(fd);
            }

            // Teeter
            {
                var bd = new BodyDef();
                bd.Position.Set(140.0f, 1.0f);
                bd.BodyType = BodyType.DynamicBody;
                var body = World.CreateBody(bd);

                var box = new PolygonShape();
                box.SetAsBox(10.0f, 0.25f);
                body.CreateFixture(box, 1.0f);

                var jd = new RevoluteJointDef();
                jd.Initialize(ground, body, body.GetPosition());
                jd.LowerAngle = -8.0f * Settings.Pi / 180.0f;
                jd.UpperAngle = 8.0f * Settings.Pi / 180.0f;
                jd.EnableLimit = true;
                World.CreateJoint(jd);

                body.ApplyAngularImpulse(100.0f, true);
            }

            // Bridge
            {
                var N = 20;
                var shape = new PolygonShape();
                shape.SetAsBox(1.0f, 0.125f);

                var fd = new FixtureDef();
                fd.Shape = shape;
                fd.Density = 1.0f;
                fd.Friction = 0.6f;

                var jd = new RevoluteJointDef();

                var prevBody = ground;
                for (var i = 0; i < N; ++i)
                {
                    var bd = new BodyDef();
                    bd.BodyType = BodyType.DynamicBody;
                    bd.Position.Set(161.0f + 2.0f * i, -0.125f);
                    var body = World.CreateBody(bd);
                    body.CreateFixture(fd);

                    var anchor = new Vector2(160.0f + 2.0f * i, -0.125f);
                    jd.Initialize(prevBody, body, anchor);
                    World.CreateJoint(jd);

                    prevBody = body;
                }

                {
                    var anchor = new Vector2(160.0f + 2.0f * N, -0.125f);
                    jd.Initialize(prevBody, ground, anchor);
                    World.CreateJoint(jd);
                }
            }

            // Boxes
            {
                var box = new PolygonShape();
                box.SetAsBox(0.5f, 0.5f);

                Body body = null;
                var bd = new BodyDef();
                bd.BodyType = BodyType.DynamicBody;

                bd.Position.Set(230.0f, 0.5f);
                body = World.CreateBody(bd);
                body.CreateFixture(box, 0.5f);

                bd.Position.Set(230.0f, 1.5f);
                body = World.CreateBody(bd);
                body.CreateFixture(box, 0.5f);

                bd.Position.Set(230.0f, 2.5f);
                body = World.CreateBody(bd);
                body.CreateFixture(box, 0.5f);

                bd.Position.Set(230.0f, 3.5f);
                body = World.CreateBody(bd);
                body.CreateFixture(box, 0.5f);

                bd.Position.Set(230.0f, 4.5f);
                body = World.CreateBody(bd);
                body.CreateFixture(box, 0.5f);
            }

            // Car
            {
                var chassis = new PolygonShape();
                var vertices = new Vector2[8];
                vertices[0].Set(-1.5f, -0.5f);
                vertices[1].Set(1.5f, -0.5f);
                vertices[2].Set(1.5f, 0.0f);
                vertices[3].Set(0.0f, 0.9f);
                vertices[4].Set(-1.15f, 0.9f);
                vertices[5].Set(-1.5f, 0.2f);
                chassis.Set(vertices);

                var circle = new CircleShape();
                circle.Radius = 0.4f;

                var bd = new BodyDef();
                bd.BodyType = BodyType.DynamicBody;
                bd.Position.Set(0.0f, 1.0f);
                _car = World.CreateBody(bd);
                _car.CreateFixture(chassis, 1.0f);

                var fd = new FixtureDef();
                fd.Shape = circle;
                fd.Density = 1.0f;
                fd.Friction = 0.9f;

                bd.Position.Set(-1.0f, 0.35f);
                _wheel1 = World.CreateBody(bd);
                _wheel1.CreateFixture(fd);

                bd.Position.Set(1.0f, 0.4f);
                _wheel2 = World.CreateBody(bd);
                _wheel2.CreateFixture(fd);

                var jd = new WheelJointDef();
                var axis = new Vector2(0.0f, 1.0f);

                jd.Initialize(_car, _wheel1, _wheel1.GetPosition(), axis);
                jd.MotorSpeed = 0.0f;
                jd.MaxMotorTorque = 20.0f;
                jd.EnableMotor = true;
                jd.FrequencyHz = _hz;
                jd.DampingRatio = _zeta;
                _spring1 = (WheelJoint)World.CreateJoint(jd);

                jd.Initialize(_car, _wheel2, _wheel2.GetPosition(), axis);
                jd.MotorSpeed = 0.0f;
                jd.MaxMotorTorque = 10.0f;
                jd.EnableMotor = false;
                jd.FrequencyHz = _hz;
                jd.DampingRatio = _zeta;
                _spring2 = (WheelJoint)World.CreateJoint(jd);
            }
        }

        protected override void PreStep()
        {
            var p = Global.Camera.Center;
            p.X = _car.GetPosition().X;
            Global.Camera.Center = p;
        }

        /// <inheritdoc />
        public override void OnKeyDown(KeyboardKeyEventArgs key)
        {
            switch (key.Key)
            {
            case Key.A:
                _spring1.SetMotorSpeed(_speed);
                break;
            case Key.S:
                _spring1.SetMotorSpeed(0.0f);
                break;
            case Key.D:
                _spring1.SetMotorSpeed(-_speed);
                break;
            case Key.Q:
                _hz = Math.Max(0.0f, _hz - 1.0f);
                _spring1.SetSpringFrequencyHz(_hz);
                _spring2.SetSpringFrequencyHz(_hz);
                break;
            case Key.E:
                _hz += 1.0f;
                _spring1.SetSpringFrequencyHz(_hz);
                _spring2.SetSpringFrequencyHz(_hz);
                break;
            }
        }

        protected override void OnRender()
        {
            DrawString("Keys: left = a, brake = s, right = d, hz down = q, hz up = e");

            DrawString($"frequency = {_hz} hz, damping ratio = {_zeta}");
        }
    }
}