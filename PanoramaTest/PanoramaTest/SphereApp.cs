using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Urho;
using Urho.Resources;
using Urho.Urho2D;

namespace PanoramaTest
{
	public class SphereApp : Application
	{
		public bool IsSceneLoaded { get; private set; }

		float yaw;
		float pitch;
		float roll;
		const float Sensitivity = .05f;
		Node cameraNode;

		public SphereApp(ApplicationOptions options) : base(options)
		{
			UnhandledException += Application_UnhandledException;
		}

		public SphereApp(IntPtr handle) : base(handle)
		{
			UnhandledException += Application_UnhandledException;
		}

		protected SphereApp(UrhoObjectFlag emptyFlag) : base(emptyFlag)
		{
			UnhandledException += Application_UnhandledException;
		}

		private void Application_UnhandledException(object sender, Urho.UnhandledExceptionEventArgs e)
		{
			e.Handled = true;
		}

		protected override async void Start()
		{
			base.Start();
			await CreateScene();
		}

		private async Task CreateScene()
		{
			// 1 - SCENE
			var scene = new Scene();
			scene.CreateComponent<Octree>();

			// 2 - NODE
			Node node = scene.CreateChild("room");
			node.Position = new Vector3(0, 0, 0);
			node.Rotation = new Quaternion(0, 0, 0);
			node.SetScale(2f);

			// 3 - MODEL OBJECT
			StaticModel modelObject = node.CreateComponent<StaticModel>();
			modelObject.Model = ResourceCache.GetModel("Models/Sphere.mdl");

			// 3.2 - ZONE
			var zoneNode = scene.CreateChild("zone");
			var zone = zoneNode.CreateComponent<Zone>();
			zone.SetBoundingBox(new BoundingBox(-300.0f, 300.0f));
			zone.AmbientColor = new Color(1f, 1f, 1f);

			// 3.5 - DOWNLOAD IMAGE
			var webClient = new WebClient() { Encoding = Encoding.UTF8 };

			// NOTE: The image MUST be in power of 2 resolution (Ex: 512x512, 2048x1024, etc...)
			var memoryBuffer = new MemoryBuffer(webClient.DownloadData(new Uri("https://video.360cities.net/littleplanet-360-imagery/360Level43Lounge-8K-stable-noaudio-2048x1024.jpg")));

			var image = new Image();
			var isLoaded = image.Load(memoryBuffer);

			if (!isLoaded)
			{
				throw new Exception();
			}

			// 3.6 TEXTURE
			var texture = new Texture2D();
			var isTextureLoaded = texture.SetData(image);

			if (!isTextureLoaded)
			{
				throw new Exception();
			}

			// 3.8 - MATERIAL
			var material = new Material();
			material.SetTexture(TextureUnit.Diffuse, texture);
			material.SetTechnique(0, CoreAssets.Techniques.DiffNormal, 0, 0);
			material.CullMode = CullMode.Cw;
			modelObject.SetMaterial(material);

			// 4 - LIGHTS
			Node light = scene.CreateChild(name: "light");
			light.SetDirection(new Vector3(0f, -0f, 0f));
			light.CreateComponent<Light>();

			// 5 - CAMERA
			cameraNode = scene.CreateChild(name: "camera");
			cameraNode.LookAt(new Vector3(0, 1, 2), new Vector3(0, 1, 0));
			Camera camera = cameraNode.CreateComponent<Camera>();
			camera.Fov = 50;

			// 6 - VIEWPORT
			Renderer.SetViewport(0, new Viewport(scene, camera, null));

			// 7 - ACTIONS
			//await node.RunActionsAsync(new RepeatForever(new RotateBy(duration: 4f, deltaAngleX: 0, deltaAngleY: 40, deltaAngleZ: 0)));
			IsSceneLoaded = true;
		}

		protected override void OnUpdate(float timeStep)
		{
			base.OnUpdate(timeStep);
			if (Input.NumTouches >= 1 && IsSceneLoaded)
			{
				var touch = Input.GetTouch(0);
				yaw += Sensitivity * touch.Delta.X;
				pitch += Sensitivity * touch.Delta.Y;
				pitch = MathHelper.Clamp(pitch, -90, 90);
				roll = 0;
				cameraNode.Rotation = new Quaternion(-pitch, -yaw, roll);
			}
		}
	}
}
