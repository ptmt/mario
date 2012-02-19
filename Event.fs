module Mario.Event 
  let listenOnce f evt =
    async {
      let! res = Async.AwaitEvent evt
      f res
    } |> Async.Start