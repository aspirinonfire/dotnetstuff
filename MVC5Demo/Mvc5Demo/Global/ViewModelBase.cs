using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mvc5Demo.Global
{
  /// <summary>
  /// Define base ViewModel object.
  /// </summary>
  public abstract class ViewModelBase
  {
    protected List<string> errors;

    public ViewModelBase()
    {
      this.errors = new List<string>();
    }

    public void addError(string error)
    {
      this.errors.Add(error);
    }

    public IEnumerable<string> getErrors()
    {
      return this.errors;
    }

  }
}