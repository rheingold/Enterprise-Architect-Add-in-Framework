﻿
using System;
using System.Collections.Generic;
using EAAddinFramework.Utilities;
using DB=DatabaseFramework;
using TSF.UmlToolingFramework.Wrappers.EA;
using System.Linq;
using UML = TSF.UmlToolingFramework.UML;
namespace EAAddinFramework.Databases
{
	/// <summary>
	/// Description of ForeignKey.
	/// </summary>
	public class ForeignKey:Constraint, DB.ForeignKey
	{
		private Association _wrappedAssociation;
		private Association _logicalAssociation;
		
		internal Table _foreignTable;
		
		public ForeignKey(Table owner,Operation operation):base(owner,operation)
		{

		}
		public ForeignKey(Table owner, List<Column> involvedColumns):base(owner, involvedColumns)
		{

		}

		#region implemented abstract members of Constraint
		protected override string getStereotype()
		{
			return "FK";
		}
		#endregion
		public override List<UML.Classes.Kernel.Element> logicalElements{
			get 
			{
				return new List<UML.Classes.Kernel.Element>(){ this.logicalAssociation};
			}
			set
			{
				this.logicalAssociation = value.OfType<Association>().FirstOrDefault();
			}
		}
		public override bool isOverridden {
			get 
			{
				//set all involved columns overridden
				if (base.isOverridden)
				{
					//if the FK is overridden then all the columns should be overridden as well
					foreach (Column involvedColumn in this.involvedColumns) 
					{
						involvedColumn.isOverridden = true;
					}
				}
				return base.isOverridden;
			}
			set 
			{
				base.isOverridden = value;
				if (value) 
				{
					//if the FK is overridden then all the columns should be overridden as well
					foreach (Column involvedColumn in this.involvedColumns) 
					{
						involvedColumn.isOverridden = true;
					}
				}
			}
		}

		internal string _onDelete;
    public virtual string onDelete {
      get {
        if( this._onDelete == null ) {
          this._onDelete = "";
          // lazy load
          foreach(var item in this._wrappedOperation.taggedValues) {
            if( item.name.Equals("Delete") ) {
              this._onDelete = (string)item.tagValue;
            }
          }
        }
        return this._onDelete;
      }
      set {
        this._onDelete = value;
        // create tagged value if needed
        // if not overridden then we don't need the tagged value;
        if( this.wrappedElement != null ) {
          this.wrappedElement.addTaggedValue("Delete", this._onDelete);
        } else if( this.logicalElement != null ) {
            ((Element)this.logicalElement).addTaggedValue(
              "Delete",
              this._onDelete
            );
        }
      }
    }

		public override void save()
		{
			base.save();

      // TODO: check this, compare to Index.save, which doesn't call base.save()
      if(this._wrappedOperation == null ) {
        this._wrappedOperation = this._factory._modelFactory.createNewElement<Operation>(this._owner._wrappedClass,this._name);
        this._wrappedOperation.setStereotype(this.getStereotype());
      }
      this._wrappedOperation.save();
      this.onDelete = this.onDelete;

			if (this.traceTaggedValue == null) createTraceTaggedValue();
			//check if association to correct table
			if (this._wrappedAssociation == null)
			{
				//create wrapped association
				this._wrappedAssociation = this._factory._modelFactory.createNewElement<Association>(this._owner._wrappedClass, "");
			}
			if (this._wrappedAssociation != null)
			{
				if (this._foreignTable._wrappedClass != null)
				{
					this._wrappedAssociation.target = this._foreignTable._wrappedClass;
					this.wrappedAssociation.sourceEnd.name = this.name;
					if (this._foreignTable.primaryKey != null) this.wrappedAssociation.targetEnd.name = this._foreignTable.primaryKey.name;
					this.wrappedAssociation.sourceEnd.EAMultiplicity = new Multiplicity("0..*");
					//TODO: implement multiplicities correctly by implementing isNotNullable on FK
					this.wrappedAssociation.targetEnd.EAMultiplicity = new Multiplicity("1..1");
					this.wrappedAssociation.save();
				}
				else
				{
					Logger.logError(string.Format("foreign table {0} has not been saved yet",_foreignTable.name));
				}
			}
		}

		#region implemented abstract members of DatabaseItem


		public override DB.DatabaseItem createAsNewItem(DB.DatabaseItem owner, bool save = true)
		{
			Table newTable = owner as Table;
			Database existingDatabase = owner as Database;
			if (newTable != null)
			{
				existingDatabase = newTable.databaseOwner as Database;
			}
			if (newTable == null)
			{
				//look for corresponding table in existingDatabase
				newTable = (Table)existingDatabase.tables.FirstOrDefault(x => x.name == this.ownerTable.name);
			}
			var newForeignTable = existingDatabase.tables.FirstOrDefault(x => x.name == this.foreignTable.name);
			if (newTable != null && newForeignTable != null)
			{
				var newForeignKey = new ForeignKey(newTable,this._involvedColumns);
				newForeignKey.name = name;
				newForeignKey._foreignTable = (Table)newForeignTable;
				newForeignKey._logicalAssociation = _logicalAssociation;
				newForeignKey.isOverridden = isOverridden;
				newForeignKey.derivedFromItem = this;
				if (save) newForeignKey.save();
				return newForeignKey;
			}
			return null;
		}


		#endregion

		protected override void updateDetails(DB.DatabaseItem newDatabaseItem)
		{
			base.updateDetails(newDatabaseItem);
			ForeignKey newForeignKey = (ForeignKey)newDatabaseItem;
			//update the association
			this._foreignTable = newForeignKey._foreignTable;
			//set logical association
			this.logicalAssociation = newForeignKey.logicalAssociation;

		}

		public override void delete()
		{
			base.delete();
			if (this._wrappedAssociation != null) this._wrappedAssociation.delete();
		}

		public Association logicalAssociation
		{
			get
			{
				if (_logicalAssociation == null 
				    && this._wrappedOperation != null)
				{
					_logicalAssociation = this._wrappedOperation.taggedValues
						.Where(x => x.name.Equals("sourceAssociation",StringComparison.InvariantCultureIgnoreCase)
						             && x.tagValue is Association)
						.Select(y => y.tagValue as Association).FirstOrDefault();
				}
				return _logicalAssociation;
			}
			set
			{
				_logicalAssociation = value;
				if (this._wrappedOperation != null)
				{
					this._wrappedOperation.addTaggedValue("sourceAssociation",value.uniqueID);
				}
			}
		}
		internal Association wrappedAssociation
		{
			get
			{
				if (_wrappedAssociation == null)
				{
					this.getWrappedAssociation();
				}
				return _wrappedAssociation;
			}
			
		}
		private void getWrappedAssociation()
		{
			if (_wrappedOperation != null)
			{
				string getAssociationQuery = @"select c.Connector_ID from t_connector c 
											where c.Start_Object_ID = " + this._owner._wrappedClass.id.ToString() +
					" and c.SourceRole = '"+ _wrappedOperation.name +"'";
				_wrappedAssociation = this._wrappedOperation.model.getRelationsByQuery(getAssociationQuery).FirstOrDefault() as Association;
			}
		}
		public override string properties {
			get 
			{
				string _properties = base.properties;
				if (foreignTable != null)
				{
					_properties += " => " + foreignTable.name;
				}
				return _properties;
			}
		}
		public override string itemType {
			get {return "Foreign Key";}
		}

		#region implemented abstract members of Constraint


		internal override void createTraceTaggedValue()
		{
			if (this._wrappedOperation != null && _logicalAssociation != null)
			{
				this._wrappedOperation.addTaggedValue("sourceAssociation",_logicalAssociation.guid);
			}
		}


		internal override TaggedValue traceTaggedValue 
		{
			get 
			{
				if (_wrappedOperation != null) return _wrappedOperation.taggedValues.OfType<TaggedValue>()
					.FirstOrDefault(x => x.name.Equals("sourceAssociation",StringComparison.InvariantCultureIgnoreCase));
				//if no wrappped operation then no tagged value
				return null;
			}
			set 
			{
				if (_wrappedOperation != null
				   && value != null)
				{
					var tag = _wrappedOperation.addTaggedValue(value.name, value.eaStringValue);
					tag.comment = value.comment;
				}
			}
		}


		#endregion

		#region ForeignKey implementation
		public DB.Table foreignTable {
			get 
			{
				if (_foreignTable == null)
				{
					this.getForeignTable();
				}
				return _foreignTable;
			}
			set 
			{
				_foreignTable = (Table)value;
			}
		}
		#endregion
		private void getForeignTable()
		{
			if (wrappedAssociation != null)
			{
				var targetClass = _wrappedAssociation.target as Class;
				if (targetClass != null)
				{
					_foreignTable = new Table(this._owner._owner, targetClass);
				}
			}
		}

	}
}
