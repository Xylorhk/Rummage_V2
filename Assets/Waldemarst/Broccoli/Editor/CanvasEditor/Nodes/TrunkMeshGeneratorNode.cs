﻿using UnityEngine;
using UnityEditor;

using Broccoli.NodeEditorFramework;
using Broccoli.NodeEditorFramework.Utilities;
using Broccoli.Pipe;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Branch mesh generator node.
	/// </summary>
	[Node (false, "Mesh Generator/Trunk Mesh Generator", 110)]
	public class TrunkMeshGeneratorNode : BaseNode 
	{
		#region Vars
		/// <summary>
		/// Gets the get Id of the node.
		/// </summary>
		/// <value>Id of the node.</value>
		public override string GetID { 
			get { return typeof (TrunkMeshGeneratorNode).ToString(); } 
		}
		/// <summary>
		/// Gets the category of the node.
		/// </summary>
		/// <value>Category of the node.</value>
		public override Category category { get { return Category.MeshGenerator; } }
		/// <summary>
		/// The branch mesh generator element.
		/// </summary>
		public TrunkMeshGeneratorElement trunkMeshGeneratorElement;
		#endregion

		#region Constructor
		/// <summary>
		/// Node constructor.
		/// </summary>
		public TrunkMeshGeneratorNode () {
			this.nodeName = "Trunk Mesh Generator";
			this.nodeHelpURL = "https://docs.google.com/document/d/1Nr6Z808i7X2zMFq8PELezPuSJNP5IvRx9C5lJxZ_Z-A/edit#heading=h.qpkoisw82dlr";
			this.nodeDescription = "This nodes provides parameters to build the trunk mesh of a tree. This section of the tree often" +
				" has displacement coming from the roots.";
		}
		#endregion

		#region Base Node
		/// <summary>
		/// Called when creating the node.
		/// </summary>
		/// <returns>The created node.</returns>
		protected override BaseNode CreateExplicit () {
			TrunkMeshGeneratorNode node = CreateInstance<TrunkMeshGeneratorNode> ();
			node.rectSize.x = 162;
			return node;
		}
		/// <summary>
		/// Sets the pipeline element of this node.
		/// </summary>
		/// <param name="pipelineElement">Pipeline element.</param>
		public override void SetPipelineElement (PipelineElement pipelineElement = null) {
			if (pipelineElement == null) {
				trunkMeshGeneratorElement = ScriptableObject.CreateInstance<TrunkMeshGeneratorElement> ();
			} else {
				trunkMeshGeneratorElement = (TrunkMeshGeneratorElement)pipelineElement;
			}
			this.pipelineElement = trunkMeshGeneratorElement;
		}
		#endregion
	}
}