using Sandbox;

namespace Sandblox
{
	public partial class Game : Sandbox.Game
	{
		[Net] private Map Map { get; set; }

		public Game()
		{
			if ( IsServer )
			{
				_ = new HudEntity();
			}
		}

		public override void PostLevelLoaded()
		{
			if ( !IsServer )
				return;

			Map = new Map();
			Map.SetSize( 256, 256, 64 );
			Map.GeneratePerlin();
			Map.Init();
		}

		public override void Spawn()
		{
			base.Spawn();
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			Map.Init();
		}

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new Player();
			client.Pawn = player;

			player.Respawn();
		}

		public void SetBlockInDirection( Vector3 pos, Vector3 dir, byte blocktype )
		{
			var face = Map.GetBlockInDirection( pos * (1.0f / Chunk.VoxelSize), dir.Normal, 10000, out var hitpos, out _ );
			if ( face == Map.BlockFace.Invalid )
				return;

			var blockPos = hitpos;

			if ( blocktype != 0 )
			{
				blockPos = Map.GetAdjacentBlockPosition( blockPos, (int)face );
			}

			SetBlockOnServer( blockPos.x, blockPos.y, blockPos.z, blocktype );
		}

		public void SetBlockOnServer( int x, int y, int z, byte blocktype )
		{
			Host.AssertServer();

			var pos = new IntVector3( x, y, z );

			if ( Map.SetBlockAndUpdate( pos, blocktype ) )
			{
				Map.WriteNetworkDataForChunkAtPosition( pos );
				SetBlockOnClient( x, y, z, blocktype );
			}
		}

		[ClientRpc]
		public void SetBlockOnClient( int x, int y, int z, byte blocktype )
		{
			Host.AssertClient();

			Map.SetBlockAndUpdate( new IntVector3( x, y, z ), blocktype, true );
		}
	}
}
