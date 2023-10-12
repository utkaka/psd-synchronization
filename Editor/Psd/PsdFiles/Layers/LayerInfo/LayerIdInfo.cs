namespace com.utkaka.PsdSynchronization.Editor.Psd.PsdFiles.Layers.LayerInfo {
	public class LayerIdInfo : AbstractLayerInfo {
		public override string Signature => "8BIM";

		public override string Key => "lyid";

		private readonly int _id;

		public int Id => _id;

		public LayerIdInfo(int id) {
			_id = id;
		}

		public LayerIdInfo(PsdBinaryReader reader) {
			_id = reader.ReadInt32();
		}

		protected override void WriteData(PsdBinaryWriter writer) {
			writer.Write(_id);
		}
	}
}