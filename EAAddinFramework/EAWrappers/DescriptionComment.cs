﻿using System;
using System.Collections.Generic;

using UML=TSF.UmlToolingFramework.UML;

namespace TSF.UmlToolingFramework.Wrappers.EA {
  public class DescriptionComment : Element, UML.Classes.Kernel.Comment {
    public override UML.Classes.Kernel.Element owner { get; set; }

    public Element EAOwner {
      get {  return this.owner as Element; }
      set {  this.owner = value; }
    }

    public DescriptionComment(Model model, Element owner) : base(model) {
      this.owner = owner;
    }
    
    public override HashSet<UML.Classes.Kernel.Element> ownedElements {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }
    
    public override HashSet<UML.Classes.Kernel.Comment> ownedComments {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }
    
    public override HashSet<UML.Profiles.Stereotype> stereotypes {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }
    
    internal override void saveElement(){
      this.EAOwner.saveElement();
    }
    
    public String body {
      get { return this.EAOwner.notes; }
      set { this.EAOwner.notes = value; }
    }
    
    public HashSet<UML.Classes.Kernel.Element> annotatedElements {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }
    
    public override String notes {
      get { return this.body;  }
      set { this.body = value; }
    }
  	
	public override TSF.UmlToolingFramework.UML.UMLItem getItemFromRelativePath(List<string> relativePath)
	{
		return null;
	}
  	
	public string name {
		get {
			return string.Empty;
		}
	}
  	
	internal override global::EA.Collection eaTaggedValuesCollection {
		get {
			throw new NotImplementedException();
		}
	}
  }
}
