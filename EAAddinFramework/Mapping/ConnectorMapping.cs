﻿using System;
using MP = MappingFramework;
using UML=TSF.UmlToolingFramework.UML;
using TSF.UmlToolingFramework.Wrappers.EA;
using System.Collections.Generic;
using System.Linq;
using TFS_EA = TSF.UmlToolingFramework.Wrappers.EA;

namespace EAAddinFramework.Mapping
{
	/// <summary>
	/// Description of ConnectorMapping.
	/// </summary>
	public class ConnectorMapping:Mapping
	{
		internal ConnectorWrapper wrappedConnector{get;private set;}
        public ConnectorMapping(ConnectorWrapper wrappedConnector, MappingNode sourceEnd, MappingNode targetEnd) : base(sourceEnd, targetEnd)
        {
            this.wrappedConnector = wrappedConnector;
        }

        #region implemented abstract members of Mapping

        public override MP.MappingLogic mappingLogic {
			get 
			{
				//check via linked element
				if (_mappingLogic == null)
				{
					//check links to notes or constraints
					//TODO: make a difference between regular notes and mapping logic?
					var connectedElement = this.wrappedConnector.getLinkedElements().OfType<ElementWrapper>().FirstOrDefault(x => x.notes.Length > 0);
					if (connectedElement != null) _mappingLogic = new MappingLogic(connectedElement);
				}
				// check via tagged value
				if (_mappingLogic == null)
				{
					var mappingtag = this.wrappedConnector.taggedValues
						.FirstOrDefault( x => x.name.Equals("mappingLogic",StringComparison.InvariantCultureIgnoreCase));
					if (mappingtag != null && 
					    ! string.IsNullOrEmpty(mappingtag.tagValue.ToString()))
					{
						_mappingLogic = new MappingLogic(mappingtag.tagValue.ToString());
					}
				}
				return _mappingLogic;
			}
			set 
			{
				var mappingElementWrapper = value.mappingElement as ElementWrapper;
				if (mappingElementWrapper != null)
				{
					this.wrappedConnector.addTaggedValue("mappinglogic",mappingElementWrapper.uniqueID);
					//TODO get this working for at least notes and constraints, for now we go with a tagged value
					// this.wrappedConnector.addLinkedElement(mappingElementWrapper);
				}
				else
				{
					this.wrappedConnector.addTaggedValue("mappingLogic",value.description);
				}
				
			}
		}

		#endregion

		#region implemented abstract members of Mapping

		public override void save()
		{
            if (this._mappingLogic != null)
                this.mappingLogic = this._mappingLogic; //make sure to set the mapping logic value correctly
            this.wrappedConnector.save();
		}

		#endregion
	}
}
