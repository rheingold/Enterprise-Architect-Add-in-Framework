﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSF.UmlToolingFramework.UML.Extended;
using MP = MappingFramework;
using TSF_EA = TSF.UmlToolingFramework.Wrappers.EA;

namespace EAAddinFramework.Mapping
{
    public class AssociationMappingNode : MappingNode
    {
        public AssociationMappingNode(TSF_EA.Association sourceAssociation, MappingSettings settings, MP.ModelStructure structure) : this(sourceAssociation, null, settings, structure) { }
        public AssociationMappingNode(TSF_EA.Association sourceAssociation, ClassifierMappingNode parent, MappingSettings settings, MP.ModelStructure structure) : base(sourceAssociation, parent, settings, structure) { }

        internal TSF_EA.Association sourceAssociation
        {
            get
            {
                return this.source as TSF_EA.Association;
            }
            set
            {
                this.source = value;
            }
        }
        public override string name
        {
            get
            {
                //check if source association has a name
                if (!string.IsNullOrEmpty(this.sourceAssociation.name))
                    return (this.sourceAssociation.name);
                //return sourceClas.sourceRole.targetRole.TargetClass
                var nameParts = new List<string>();
                if (!string.IsNullOrEmpty(this.sourceAssociation.sourceName)) nameParts.Add(this.sourceAssociation.sourceName);
                if (!string.IsNullOrEmpty(this.sourceAssociation.sourceEnd.name)) nameParts.Add(this.sourceAssociation.sourceEnd.name);
                if (!string.IsNullOrEmpty(this.sourceAssociation.targetEnd.name)) nameParts.Add(this.sourceAssociation.targetEnd.name);
                if (!string.IsNullOrEmpty(this.sourceAssociation.targetName)) nameParts.Add(this.sourceAssociation.targetName);
                return string.Join(".", nameParts);
            }
        }

        public override IEnumerable<MP.Mapping> getOwnedMappings(MP.MappingNode targetRootNode)
        {
            //the mappings are stored in a tagged value
            var foundMappings = new List<MP.Mapping>();
            //Mappings are stored in tagged values
            foreach (var mappingTag in this.sourceAssociation.taggedValues.Where(x => x.name == this.settings.linkedAttributeTagName))
            {
                var mapping = MappingFactory.getMapping(this, (TSF_EA.TaggedValue)mappingTag, (MappingNode)targetRootNode);
                if (mapping != null) foundMappings.Add(mapping);
            }
            //loop subNodes
            foreach (MappingNode childNode in this.allChildNodes)
            {
                foundMappings.AddRange(childNode.getOwnedMappings(targetRootNode));
            }
            return foundMappings;
        }

        public override void setChildNodes()
        {
            //we only traverse associations in case of message structrure
            if (this.structure == MP.ModelStructure.Message)
            {
                //create mapping node for target element
                var targetElement = this.sourceAssociation.targetElement as TSF_EA.ElementWrapper;
                if (targetElement != null && !this.allChildNodes.Any(x => x.source?.uniqueID == targetElement.uniqueID))
                {
                    var childNode = new ClassifierMappingNode(targetElement, this, this.settings, this.structure);
                }
            }
        }

        protected override UMLItem createMappingItem(MappingNode targetNode)
        {
            return this.createTaggedValueMappingItem(targetNode);
        }
    }
}
